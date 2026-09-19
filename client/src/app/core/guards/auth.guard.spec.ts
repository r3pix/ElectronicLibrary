import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let authService: { isAuthenticated: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(() => {
    authService = { isAuthenticated: vi.fn() };
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authService }]
    });
    router = TestBed.inject(Router);
  });

  function run() {
    return TestBed.runInInjectionContext(() => authGuard({} as any, { url: '/assets' } as any));
  }

  it('allows navigation when the user is authenticated', () => {
    authService.isAuthenticated.mockReturnValue(true);

    expect(run()).toBe(true);
  });

  it('redirects to /login when the user is not authenticated', () => {
    authService.isAuthenticated.mockReturnValue(false);

    const result = run();

    expect(result).toEqual(router.createUrlTree(['/login']));
  });
});
