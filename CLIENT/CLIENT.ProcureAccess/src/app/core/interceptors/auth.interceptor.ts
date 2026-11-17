// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  HttpContextToken,
  HttpErrorResponse,
  HttpHeaders,
  HttpInterceptorFn,
  HttpResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, tap, throwError } from 'rxjs';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';
import { LoginOverlayService } from '@app/core/session/login-overlay.service';
import {
  SESSION_ABSOLUTE_EXPIRES_AT_HEADER,
  SESSION_EXPIRES_AT_HEADER,
  SessionLimits,
} from '@app/core/session/session-limits';

/** Set on a request that was already retried after a sign-in: a second 401 is not retried (no loop). */
export const RETRIED_AFTER_SIGN_IN = new HttpContextToken<boolean>(() => false);

/**
 * The session interceptor (ARCHITECTURE §8.3, C60; SEC-03, SEC-07).
 *
 * Expiry headers: every answer to a signed-in request, also a failed one, names the two limits of
 * the session in `X-Session-Expires-At` and `X-Session-Absolute-Expires-At`, because the request
 * renewed the idle limit; they go into {@link SessionLimits} for the expiry warning. An answer
 * without them (anonymous, a 401, the sign-out) changes nothing here: the store forgets the limits
 * with the session.
 *
 * 401: one from any endpoint opens the login overlay without navigation, holds the failed request
 * and sends it again once after a successful sign-in; the user's page, input and draft stay. The
 * failure is passed on when the overlay is closed instead, and when the retry fails too.
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
  const limits = inject(SessionLimits);
  const apiUrl = inject(API_URL);
  const isProbe = req.url === `${apiUrl}/v1/auth/session`;

  const readLimits = (headers: HttpHeaders): void => {
    const expiresAt = headers.get(SESSION_EXPIRES_AT_HEADER);
    if (expiresAt !== null) {
      limits.set(expiresAt, headers.get(SESSION_ABSOLUTE_EXPIRES_AT_HEADER));
    }
  };

  return next(req).pipe(
    tap({
      next: (event) => {
        if (event instanceof HttpResponse) {
          readLimits(event.headers);
        }
      },
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse) {
          readLimits(error.headers);
        }
      },
    }),
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
