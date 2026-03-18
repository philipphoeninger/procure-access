import type { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { JWT_NAME } from '@app/app.config';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const jwtName = inject(JWT_NAME);
  const jwt = localStorage.getItem(jwtName);
  if (jwt !== null) {
    req = req.clone({
      headers: req.headers.set('Authorization', 'bearer ' + jwt),
    });
  }
  return next(req);
};
