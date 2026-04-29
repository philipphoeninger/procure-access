import type { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { JWT_NAME, REFRESH_TOKEN_NAME } from '@app/app.config';
import { AuthService } from '@app/features/identity/services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

const appendJwt = (req: HttpRequest<unknown>) => {
  const jwtName = inject(JWT_NAME);
  const jwt = localStorage.getItem(jwtName);
  if (jwt != null) {
    req = req.clone({
      headers: req.headers.set('Authorization', 'bearer ' + jwt),
    });
  }
  return req;
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const refreshTokenName = inject(REFRESH_TOKEN_NAME);
  const auth = inject(AuthService);
  
  req = appendJwt(req);

  // skip for refresh endpoint
  if (req.url.includes('/refresh')) return next(req);
  // try refresh on 401
  const refreshToken = localStorage.getItem(refreshTokenName);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // only handle 401
      if (error.status !== 401 || refreshToken == null) {
        return throwError(() => error);
      }
      // try refresh
      return auth.refreshToken(refreshToken).pipe(
        switchMap(response => {
          if (!response.accessToken) {
            auth.logout(); // ❌ refresh failed
            return throwError(() => error);
          }
          // use new tokens
          const jwtName = inject(JWT_NAME);
          localStorage.setItem(jwtName, response.accessToken);
          localStorage.setItem(refreshTokenName, response.refreshToken);
          req = appendJwt(req);

          // retry original request
          return next(req);
        })
      );
    })
  );
};
