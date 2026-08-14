import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Ensures user is authenticated before accessing a route.
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/**
 * Ensures user has required role(s) before accessing a route.
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.hasRole(allowedRoles)) {
      return true;
    }

    // Redirect to default dashboard based on user role if unauthorized
    const userRole = authService.getUserRole();
    if (userRole === 'admin') {
      return router.createUrlTree(['/adminDashboard']);
    } else if (userRole === 'librarian') {
      return router.createUrlTree(['/librarianDashboard']);
    } else {
      return router.createUrlTree(['/studentDashboard']);
    }
  };
};
