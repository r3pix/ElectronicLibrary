import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AccessTokenResponse,
  CurrentUserModel,
  LoginRequest,
  RefreshRequest,
  RegisterRequest
} from '../models/auth.model';
import { ApiResponse } from '../models/response.model';

const STORAGE_KEY = 'electroniclibrary.auth';

interface StoredTokens {
  accessToken: string;
  refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly accessTokenSig = signal<string | null>(this.readStoredTokens()?.accessToken ?? null);
  private readonly refreshTokenSig = signal<string | null>(this.readStoredTokens()?.refreshToken ?? null);
  readonly currentUser = signal<CurrentUserModel | null>(null);

  readonly isAuthenticated = computed(() => this.accessTokenSig() !== null);
  readonly isAdmin = computed(() => this.currentUser()?.roles.includes('Admin') ?? false);

  accessToken(): string | null {
    return this.accessTokenSig();
  }

  refreshToken(): string | null {
    return this.refreshTokenSig();
  }

  register(request: RegisterRequest): Observable<void> {
    // Custom endpoint, not MapIdentityApi's built-in /register — that one only accepts email/password,
    // no room for first/last name.
    return this.http.post<void>(`${environment.apiBaseUrl}/api/auth/register`, request);
  }

  login(request: LoginRequest): Observable<AccessTokenResponse> {
    return this.http
      .post<AccessTokenResponse>(`${environment.apiBaseUrl}/login?useCookies=false`, request)
      .pipe(tap((tokens) => this.setTokens(tokens)));
  }

  refresh(): Observable<AccessTokenResponse> {
    const refreshToken = this.refreshTokenSig();
    if (!refreshToken) {
      throw new Error('No refresh token available.');
    }
    const body: RefreshRequest = { refreshToken };
    return this.http
      .post<AccessTokenResponse>(`${environment.apiBaseUrl}/refresh`, body)
      .pipe(tap((tokens) => this.setTokens(tokens)));
  }

  loadCurrentUser(): Observable<CurrentUserModel> {
    return this.http
      .get<ApiResponse<CurrentUserModel>>(`${environment.apiBaseUrl}/api/users/me`)
      .pipe(
        map((response) => response.result),
        tap((user) => this.currentUser.set(user))
      );
  }

  logout(): void {
    this.accessTokenSig.set(null);
    this.refreshTokenSig.set(null);
    this.currentUser.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  private setTokens(tokens: AccessTokenResponse): void {
    this.accessTokenSig.set(tokens.accessToken);
    this.refreshTokenSig.set(tokens.refreshToken);
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ accessToken: tokens.accessToken, refreshToken: tokens.refreshToken })
    );
  }

  private readStoredTokens(): StoredTokens | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as StoredTokens;
    } catch {
      return null;
    }
  }
}
