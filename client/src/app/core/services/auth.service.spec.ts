import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AccessTokenResponse, CurrentUserModel } from '../models/auth.model';
import { ApiResponse } from '../models/response.model';
import { AuthService } from './auth.service';

const STORAGE_KEY = 'electroniclibrary.auth';

describe('AuthService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('starts unauthenticated when nothing is stored', () => {
    const service = TestBed.inject(AuthService);

    expect(service.isAuthenticated()).toBe(false);
    expect(service.accessToken()).toBeNull();
    expect(service.refreshToken()).toBeNull();
  });

  it('restores tokens already present in localStorage on construction', () => {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' })
    );

    const service = TestBed.inject(AuthService);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.accessToken()).toBe('stored-access');
    expect(service.refreshToken()).toBe('stored-refresh');
  });

  it('tolerates corrupt JSON in localStorage and starts unauthenticated', () => {
    localStorage.setItem(STORAGE_KEY, '{not-json');

    const service = TestBed.inject(AuthService);

    expect(service.isAuthenticated()).toBe(false);
  });

  it('login() stores the returned tokens and flips isAuthenticated', () => {
    const service = TestBed.inject(AuthService);
    const tokens: AccessTokenResponse = {
      tokenType: 'Bearer',
      accessToken: 'new-access',
      expiresIn: 3600,
      refreshToken: 'new-refresh'
    };

    service.login({ email: 'user@example.com', password: 'pw' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/login?useCookies=false`);
    expect(req.request.method).toBe('POST');
    req.flush(tokens);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.accessToken()).toBe('new-access');
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY)!)).toEqual({
      accessToken: 'new-access',
      refreshToken: 'new-refresh'
    });
  });

  it('refresh() throws synchronously when no refresh token is available', () => {
    const service = TestBed.inject(AuthService);

    expect(() => service.refresh()).toThrow('No refresh token available.');
  });

  it('refresh() posts the stored refresh token and stores the renewed tokens', () => {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ accessToken: 'old-access', refreshToken: 'refresh-1' })
    );
    const service = TestBed.inject(AuthService);

    service.refresh().subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/refresh`);
    expect(req.request.body).toEqual({ refreshToken: 'refresh-1' });
    req.flush({ tokenType: 'Bearer', accessToken: 'renewed-access', expiresIn: 3600, refreshToken: 'refresh-2' });

    expect(service.accessToken()).toBe('renewed-access');
    expect(service.refreshToken()).toBe('refresh-2');
  });

  it('loadCurrentUser() unwraps the response and isAdmin reflects the Admin role', () => {
    const service = TestBed.inject(AuthService);
    const user: CurrentUserModel = {
      email: 'admin@example.com',
      firstName: 'Ada',
      lastName: 'Min',
      roles: ['Admin']
    };

    service.loadCurrentUser().subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/users/me`);
    req.flush({ result: user, code: 200, message: '', isError: false } satisfies ApiResponse<CurrentUserModel>);

    expect(service.currentUser()).toEqual(user);
    expect(service.isAdmin()).toBe(true);
  });

  it('isAdmin is false for a user without the Admin role', () => {
    const service = TestBed.inject(AuthService);
    const user: CurrentUserModel = { email: 'user@example.com', firstName: 'U', lastName: 'Ser', roles: ['User'] };

    service.loadCurrentUser().subscribe();

    httpMock
      .expectOne(`${environment.apiBaseUrl}/api/users/me`)
      .flush({ result: user, code: 200, message: '', isError: false } satisfies ApiResponse<CurrentUserModel>);

    expect(service.isAdmin()).toBe(false);
  });

  it('logout() clears tokens, current user, and localStorage', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ accessToken: 'a', refreshToken: 'b' }));
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(true);

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.accessToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });
});
