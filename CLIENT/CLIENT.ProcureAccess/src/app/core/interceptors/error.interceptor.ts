// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { SnackbarService } from '../services/snackbar.service';
import { ERROR_TRANSLATIONS } from '../models/error-translation.map';
import { FORM_SHOWS_FIELD_ERRORS, PROBLEM_TYPE_KEYS, problemTypeName } from '../errors/problem-details';

/** Error body of the legacy identity endpoints (until migration step 3). */
interface LegacyErrorBody {
  code?: string;
  errors?: { code?: string }[];
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const translate = inject(TranslateService);
  const snackbar = inject(SnackbarService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const problemType = problemTypeName(error);

      // The form shows these at its fields and in its error summary (API-02).
      if (problemType === 'validation' && req.context.get(FORM_SHOWS_FIELD_ERRORS)) {
        return throwError(() => error);
      }

      // Problem Details name a type (SPEC §8.6); the legacy identity endpoints still send `code`.
      const legacyBody = error?.error as LegacyErrorBody | null | undefined;
      const errorCode =
        legacyBody?.code ||
        legacyBody?.errors?.[0]?.code ||
        'Default';
      const key =
        (problemType && PROBLEM_TYPE_KEYS[problemType]) ||
        ERROR_TRANSLATIONS[errorCode] ||
        ERROR_TRANSLATIONS['Default'];

      return translate.get(key).pipe(
        switchMap((message: string) => {
          snackbar.showError(message);

          return throwError(() => error); // keep error flow
        })
      );
    })
  );
};
