import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpRequest,
  HttpHandlerFn
} from '@angular/common/http';

import { inject } from '@angular/core';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';

import { AuthFacadeService } from '../services/auth/auth-facade.service';

// Global refresh state shared between requests.
let refreshInProgress = false;

const refreshTokenSubject = new BehaviorSubject<string | null>(null);

/**
 * HTTP interceptor that:
 * 1. Adds the access token to protected requests.
 * 2. Handles 401 responses by refreshing the access token.
 * 3. Retries the failed request with the new access token.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthFacadeService);

  // Login and refresh must be sent without an access token.
  if (isAnonymousAuthEndpoint(req.url)) {
    return next(req);
  }

  const accessToken = auth.getAccessToken();
  let authReq = req;

  if (accessToken) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((err) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        return handle401Error(authReq, next, auth);
      }

      return throwError(() => err);
    })
  );
};

/**
 * Returns true only for authentication endpoints
 * that do not require an access token.
 */
function isAnonymousAuthEndpoint(url: string): boolean {
  const normalizedUrl = url.toLowerCase();

  return (
    normalizedUrl.includes('/auth/login') ||
    normalizedUrl.includes('/auth/refresh')
  );
}

/**
 * Handles a 401 response by refreshing the access token
 * and retrying the original request.
 */
function handle401Error(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  auth: AuthFacadeService
): Observable<any> {
  const refreshToken = auth.getRefreshToken();

  if (!refreshToken) {
    auth.redirectToLogin();
    return throwError(() => new Error('No refresh token available.'));
  }

  if (refreshInProgress) {
    return refreshTokenSubject.pipe(
      filter((token) => token !== null),
      take(1),
      switchMap((token) => {
        const clonedRequest = token
          ? req.clone({
              setHeaders: {
                Authorization: `Bearer ${token}`
              }
            })
          : req;

        return next(clonedRequest);
      })
    );
  }

  refreshInProgress = true;
  refreshTokenSubject.next(null);

  return auth.refresh({
    refreshToken,
    fingerprint: null
  }).pipe(
    switchMap((response) => {
      refreshInProgress = false;

      const newAccessToken = response.accessToken;

      refreshTokenSubject.next(newAccessToken);

      const clonedRequest = req.clone({
        setHeaders: {
          Authorization: `Bearer ${newAccessToken}`
        }
      });

      return next(clonedRequest);
    }),

    catchError((error) => {
      refreshInProgress = false;
      refreshTokenSubject.next(null);

      auth.redirectToLogin();

      return throwError(() => error);
    })
  );
}