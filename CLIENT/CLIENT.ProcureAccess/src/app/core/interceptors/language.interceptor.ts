// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { HttpInterceptorFn } from '@angular/common/http';
import { DEFAULT_LANGUAGE } from '@app/app.config';

export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  req = req.clone({
    setHeaders: {
      'Accept-Language': DEFAULT_LANGUAGE
    }
  });
  return next(req);
};
