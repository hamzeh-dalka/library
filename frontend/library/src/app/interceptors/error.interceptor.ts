import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * A 401 means the JWT is missing/expired/invalid - the API rejected the request
 * before it ever reached a controller. Rather than letting every component
 * silently fail (blank list, stuck spinner), clear the stale token and bounce
 * the user back to login so they can re-authenticate.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        const wasLoggedIn = authService.isLoggedIn();
        authService.logout();

        if (wasLoggedIn && !router.url.startsWith('/login')) {
          router.navigate(['/login'], {
            queryParams: { returnUrl: router.url, sessionExpired: true }
          });
        }
      }

      return throwError(() => err);
    })
  );
};
