// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { HttpInterceptorFn } from '@angular/common/http';

export const headersInterceptor: HttpInterceptorFn = (req, next) => {
  req = req.clone({
    headers: req.headers
      .set('Content-Type', 'application/json')
      .set('Access-Control-Allow-Headers', 'Content-Type, Authorization'),
  });
  return next(req);
};

