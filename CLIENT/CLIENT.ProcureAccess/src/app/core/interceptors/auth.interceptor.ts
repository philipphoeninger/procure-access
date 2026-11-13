// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';
import { LoginOverlayService } from '@app/core/session/login-overlay.service';

/** Set on a request that was already retried after a sign-in: a second 401 is not retried (no loop). */
export const RETRIED_AFTER_SIGN_IN = new HttpContextToken<boolean>(() => false);

/**
 * The session interceptor (ARCHITECTURE §8.3, C60; SEC-03): a 401 from any endpoint opens the
 * login overlay without navigation, holds the failed request and sends it again once after a
 * successful sign-in; the user's page, input and draft stay. The failure is passed on when the
 * overlay is closed instead, and when the retry fails too.
 *
 * Not retried: the session probe (it answers 200 for everyone, a 401 from it is unexpected and
 * ignored by the caller) and a request the user did not ask for (`BACKGROUND_REQUEST`), which
 * must not open a dialog.
 *
 * The antiforgery header is Angular's own XSRF interceptor (`withXsrfConfiguration` in
 * `app.config.ts`): it copies the cookie `XSRF-TOKEN` of the probe into `X-XSRF-TOKEN` on every
 * state-changing request to a relative URL. There is no token in JavaScript (SEC-01).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const overlay = inject(LoginOverlayService);
  const apiUrl = inject(API_URL);
  const isProbe = req.url === `${apiUrl}/v1/auth/session`;

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (
        error.status !== 401 ||
        isProbe ||
        req.context.get(BACKGROUND_REQUEST) ||
        req.context.get(RETRIED_AFTER_SIGN_IN)
      ) {
        return throwError(() => error);
      }
      return from(overlay.requestSignIn()).pipe(
        switchMap((signedIn) => {
          if (!signedIn) {
            return throwError(() => error);
          }
          req.context.set(RETRIED_AFTER_SIGN_IN, true);
          return next(req);
        }),
      );
    }),
  );
};
