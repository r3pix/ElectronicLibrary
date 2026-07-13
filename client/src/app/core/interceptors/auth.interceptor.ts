import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

const AUTH_ENDPOINTS = ['/login', '/register', '/refresh'];

// Module-level: shared across every request made through this single functional interceptor instance,
// so concurrent 401s during a refresh all wait on the same in-flight refresh instead of each starting
// their own.
let isRefreshing = false;
const refreshedToken$ = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isApiRequest = req.url.startsWith(environment.apiBaseUrl);
  const isAuthEndpoint = AUTH_ENDPOINTS.some((suffix) => req.url.includes(suffix));

  const token = authService.accessToken();
  const authorizedReq =
    isApiRequest && token && !isAuthEndpoint ? withBearer(req, token) : req;

  return next(authorizedReq).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && isApiRequest && !isAuthEndpoint) {
        return handleUnauthorized(req, next, authService, router);
      }
      return throwError(() => error);
    })
  );
};

function handleUnauthorized(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<HttpEvent<unknown>> {
  if (!authService.refreshToken()) {
    authService.logout();
    void router.navigate(['/login']);
    return throwError(() => new Error('Not authenticated.'));
  }

  if (!isRefreshing) {
    isRefreshing = true;
    refreshedToken$.next(null);

    return authService.refresh().pipe(
      switchMap((tokens) => {
        isRefreshing = false;
        refreshedToken$.next(tokens.accessToken);
        return next(withBearer(req, tokens.accessToken));
      }),
      catchError((refreshError: unknown) => {
        isRefreshing = false;
        authService.logout();
        void router.navigate(['/login']);
        return throwError(() => refreshError);
      })
    );
  }

  return refreshedToken$.pipe(
    filter((token): token is string => token !== null),
    take(1),
    switchMap((token) => next(withBearer(req, token)))
  );
}

function withBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
