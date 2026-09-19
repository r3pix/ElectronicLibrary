import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';

// The real environment.ts (used by `ng test` — no fileReplacements on the test target) ships with
// apiBaseUrl: '' — every string "starts with" '', which would make isApiRequest always true and
// defeat the very boundary this suite needs to exercise. `environment` is a plain shared object
// (not a DI token, so it can't be overridden via TestBed providers) — mutate its property directly
// to a real origin for the duration of this suite and restore it afterwards.
const originalApiBaseUrl = environment.apiBaseUrl;
const API_BASE_URL = 'https://api.test.local';

const STORAGE_KEY = 'electroniclibrary.auth';
const PROTECTED_URL = `${API_BASE_URL}/api/assets`;

function seedTokens(accessToken: string, refreshToken: string | null): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ accessToken, refreshToken }));
}

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    environment.apiBaseUrl = API_BASE_URL;
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
    environment.apiBaseUrl = originalApiBaseUrl;
  });

  it('attaches the bearer token to an authenticated Api request', () => {
    seedTokens('access-1', 'refresh-1');

    http.get(PROTECTED_URL).subscribe();

    const req = httpMock.expectOne(PROTECTED_URL);
    expect(req.request.headers.get('Authorization')).toBe('Bearer access-1');
    req.flush({});
  });

  it('does not attach a header when there is no token stored', () => {
    http.get(PROTECTED_URL).subscribe();

    const req = httpMock.expectOne(PROTECTED_URL);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('does not attach a header to auth endpoints even when a token exists', () => {
    seedTokens('access-1', 'refresh-1');

    http.post(`${API_BASE_URL}/login?useCookies=false`, {}).subscribe();

    const req = httpMock.expectOne(`${API_BASE_URL}/login?useCookies=false`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('does not attach a header to requests outside the Api base URL', () => {
    seedTokens('access-1', 'refresh-1');

    http.put('https://blob.example.com/container/file.pdf', 'x').subscribe();

    const req = httpMock.expectOne('https://blob.example.com/container/file.pdf');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('on a single 401, refreshes once and retries the original request with the new token', () => {
    seedTokens('expired-access', 'refresh-1');
    let result: unknown;

    http.get(PROTECTED_URL).subscribe((r) => (result = r));

    httpMock.expectOne(PROTECTED_URL).flush('unauthorized', { status: 401, statusText: 'Unauthorized' });

    const refreshReq = httpMock.expectOne(`${API_BASE_URL}/refresh`);
    expect(refreshReq.request.body).toEqual({ refreshToken: 'refresh-1' });
    refreshReq.flush({ tokenType: 'Bearer', accessToken: 'new-access', expiresIn: 3600, refreshToken: 'new-refresh' });

    const retryReq = httpMock.expectOne(PROTECTED_URL);
    expect(retryReq.request.headers.get('Authorization')).toBe('Bearer new-access');
    retryReq.flush({ ok: true });

    expect(result).toEqual({ ok: true });
  });

  it('dedupes concurrent 401s into a single refresh call and retries every request', () => {
    seedTokens('expired-access', 'refresh-1');
    let resultA: unknown;
    let resultB: unknown;

    http.get(PROTECTED_URL).subscribe((r) => (resultA = r));
    http.get(PROTECTED_URL).subscribe((r) => (resultB = r));

    const initial = httpMock.match(PROTECTED_URL);
    expect(initial).toHaveLength(2);
    initial.forEach((r) => r.flush('unauthorized', { status: 401, statusText: 'Unauthorized' }));

    const refreshReqs = httpMock.match(`${API_BASE_URL}/refresh`);
    expect(refreshReqs).toHaveLength(1);
    refreshReqs[0].flush({ tokenType: 'Bearer', accessToken: 'new-access', expiresIn: 3600, refreshToken: 'new-refresh' });

    const retries = httpMock.match(PROTECTED_URL);
    expect(retries).toHaveLength(2);
    retries.forEach((r) => {
      expect(r.request.headers.get('Authorization')).toBe('Bearer new-access');
      r.flush({ ok: true });
    });

    expect(resultA).toEqual({ ok: true });
    expect(resultB).toEqual({ ok: true });
  });

  it('on 401 with no refresh token, logs out and redirects to login without calling refresh', () => {
    seedTokens('expired-access', null);

    http.get(PROTECTED_URL).subscribe({ error: () => undefined });

    httpMock.expectOne(PROTECTED_URL).flush('unauthorized', { status: 401, statusText: 'Unauthorized' });

    httpMock.expectNone(`${API_BASE_URL}/refresh`);
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('if the refresh call itself fails, logs out and redirects to login', () => {
    seedTokens('expired-access', 'refresh-1');

    http.get(PROTECTED_URL).subscribe({ error: () => undefined });

    httpMock.expectOne(PROTECTED_URL).flush('unauthorized', { status: 401, statusText: 'Unauthorized' });
    httpMock
      .expectOne(`${API_BASE_URL}/refresh`)
      .flush('refresh failed', { status: 401, statusText: 'Unauthorized' });

    expect(router.navigate).toHaveBeenCalledWith(['/login']);
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });
});
