import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { AuthService } from '../services/auth.service';
import { guestGuard } from './guest.guard';

describe('guestGuard', () => {
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
    return TestBed.runInInjectionContext(() => guestGuard({} as any, { url: '/login' } as any));
  }

  it('allows navigation to login/register when the user is not authenticated', () => {
    authService.isAuthenticated.mockReturnValue(false);

    expect(run()).toBe(true);
  });

  it('redirects an already-authenticated user to the grid', () => {
    authService.isAuthenticated.mockReturnValue(true);

    const result = run();

    expect(result).toEqual(router.createUrlTree(['/']));
  });
});
