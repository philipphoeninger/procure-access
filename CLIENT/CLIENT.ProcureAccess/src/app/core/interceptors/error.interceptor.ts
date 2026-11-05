// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse, HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_URL } from '@app/app.config';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { catchError, switchMap, tap, throwError } from 'rxjs';
import { SnackbarService } from '../services/snackbar.service';
import { ERROR_TRANSLATIONS } from '../models/error-translation.map';
import { FORM_SHOWS_FIELD_ERRORS, PROBLEM_TYPE_KEYS, problemTypeName } from '../errors/problem-details';
import { ConnectionStatus, SERVER_ERROR_PATH } from '../errors/system-errors';

/** Error body of the legacy identity endpoints (until migration step 3). */
interface LegacyErrorBody {
  code?: string;
  errors?: { code?: string }[];
}

/**
 * System states of Handoff §6.5 for every request (R-SHL-08): no answer at all (status 0) shows
 * the offline notice, and only an answer of the API (`API_URL`) hides it again, because static
 * files such as `i18n/de.json` come from the web server even when the API is down (#171); a 5xx
 * answer shows the S80 page „Etwas ist schiefgelaufen“ in place of the current page without
 * changing the URL (owner decision S2-17, 3A). Neither gets a snackbar. Other errors keep the snackbar until their screen is rebuilt.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const translate = inject(TranslateService);
  const snackbar = inject(SnackbarService);
  const connection = inject(ConnectionStatus);
  const router = inject(Router);
  const apiUrl = inject(API_URL);
  const isApiRequest = req.url === apiUrl || req.url.startsWith(`${apiUrl}/`);
  const answered = () => {
    if (isApiRequest) connection.answered();
  };

  return next(req).pipe(
    tap((event) => {
      if (event instanceof HttpResponse) answered();
    }),
    catchError((error: HttpErrorResponse) => {
      if (error.status === 0) {
        connection.noAnswer();
        return throwError(() => error);
      }
      answered();
      if (error.status >= 500) {
        void router.navigate([SERVER_ERROR_PATH], { skipLocationChange: true });
        return throwError(() => error);
      }

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
