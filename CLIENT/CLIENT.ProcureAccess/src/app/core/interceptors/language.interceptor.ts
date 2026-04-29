import type { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LanguageService } from '@features/settings/services/language.service';

export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  const languageService = inject(LanguageService);

  req = req.clone({
    setHeaders: {
      'Accept-Language': languageService.get()
    }
  });
  return next(req);
};
