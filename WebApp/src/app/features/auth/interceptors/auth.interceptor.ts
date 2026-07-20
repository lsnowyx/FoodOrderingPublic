import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthStore } from '../services/auth-store.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);
  const token = request.url.startsWith('/api/')
    ? authStore.getValidToken()
    : null;
  const authenticatedRequest = token === null
    ? request
    : request.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {
      if (
        typeof error === 'object'
        && error !== null
        && 'status' in error
        && error.status === 401
      ) {
        const hadSession = authStore.currentSession() !== null;
        authStore.logout();

        if (hadSession && !router.url.startsWith('/login')) {
          void router.navigate(['/login'], {
            queryParams: { returnUrl: router.url }
          });
        }
      }

      return throwError(() => error);
    })
  );
};
