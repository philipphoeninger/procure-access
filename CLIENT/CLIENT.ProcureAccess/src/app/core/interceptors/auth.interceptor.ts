import type { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const jwt = localStorage.getItem('procure-access-token');
  if (jwt !== null) {
    req = req.clone({
      headers: req.headers.set('Authorization', 'bearer ' + jwt),
    });
  }
  return next(req);
};

