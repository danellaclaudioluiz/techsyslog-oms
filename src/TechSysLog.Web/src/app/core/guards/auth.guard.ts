import { inject } from '@angular/core';
import { Router, CanActivateFn, CanMatchFn } from '@angular/router';
import { AuthService } from '@core/services';
import { UserRole } from '@core/models';

// ============================================================================
// Auth Guards (Functional)
// ============================================================================

/**
 * Requires user to be authenticated
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/auth/login'], {
    queryParams: { returnUrl: router.url },
  });
  return false;
};

/**
 * Requires user to NOT be authenticated (for login/register pages)
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/dashboard']);
  return false;
};

/**
 * Creates a guard that requires specific roles
 */
export function roleGuard(allowedRoles: UserRole[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      router.navigate(['/auth/login']);
      return false;
    }

    if (authService.hasAnyRole(allowedRoles)) {
      return true;
    }

    // User is logged in but doesn't have permission
    router.navigate(['/dashboard']);
    return false;
  };
}

/**
 * Requires Admin role
 */
export const adminGuard: CanActivateFn = roleGuard(['Admin']);

/**
 * Requires Admin or Operator role
 */
export const operatorGuard: CanActivateFn = roleGuard(['Admin', 'Operator']);

/**
 * Can match guard for lazy loaded routes
 */
export const authMatchGuard: CanMatchFn = () => {
  const authService = inject(AuthService);
  return authService.isAuthenticated();
};
