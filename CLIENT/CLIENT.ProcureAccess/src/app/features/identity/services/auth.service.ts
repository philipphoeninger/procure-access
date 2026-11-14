// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext, HttpResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { lastValueFrom, Observable } from 'rxjs';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST, CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';
import { LOGIN_PATH, LoginNavigationState, LoginReason, REGISTER_PATH } from '../models/login-navigation';
import { ResetPasswordModel } from '../models/reset-password.model';
import { SessionResponse } from '../models/session';
import { User } from '../models/user.model';

/** Problem types of `POST /api/v1/auth/login` that the sign-in form shows itself (SPEC §8.8). */
export const LOGIN_PROBLEMS: readonly string[] = ['validation', 'antiforgery'];

/** Problem types of `POST /api/v1/auth/register` that S02 shows in its error summary (SPEC §8.8). */
export const REGISTER_PROBLEMS: readonly string[] = ['validation', 'antiforgery', 'rate-limit'];

/** Problem types of `POST /api/v1/auth/confirm-email/resend` that the notice shows itself (SPEC §8.8). */
export const RESEND_PROBLEMS: readonly string[] = ['email-delivery', 'rate-limit'];

/** The body of `POST /api/v1/auth/register` (SPEC §8.8). */
export interface RegistrationRequest {
  email: string;
  password: string;
  privacyAcknowledged: boolean;
}

/**
 * HTTP calls of the session and the legacy account flows (ARCHITECTURE §8.3, §9.1; S5-08).
 *
 * The session is the Identity cookie: the browser sends it on its own, the client never holds a
 * token (SEC-01). The three session calls are `GET /api/v1/auth/session` (the probe, also the
 * source of the antiforgery cookie), `POST /api/v1/auth/login` and `POST /api/v1/auth/logout`
 * (SPEC §8.8). The state lives in `withIdentity()`, which is the only caller of them.
 *
 * Registration and address confirmation (S5-12b, SPEC §8.8): `register`, `confirmEmail` and
 * `resendConfirmation`. `forgotPassword` and `resetPassword` still call the legacy identity
 * endpoints until S5-13 replaces their screens.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  /**
   * The session probe. Marked {@link BACKGROUND_REQUEST}: the user did not ask for it, so a failure
   * shows nothing; the caller decides what a failed probe means (nobody signed in).
   */
  session(): Promise<SessionResponse> {
    return lastValueFrom(
      this.http.get<SessionResponse>(`${this.apiUrl}/v1/auth/session`, {
        context: new HttpContext().set(BACKGROUND_REQUEST, true),
      }),
    );
  }

  /**
   * Signs in; the answer is 204 with the session cookie, or 400 `validation` with one error for
   * the body as a whole (wrong address or password, locked or deleted account; SPEC §8.8). The
   * sign-in form shows that and a missing antiforgery token in place ({@link LOGIN_PROBLEMS});
   * every other failure keeps the system states of the error interceptor.
   */
  login(email: string, password: string): Promise<void> {
    return lastValueFrom(
      this.http.post<void>(
        `${this.apiUrl}/v1/auth/login`,
        { email, password },
        { context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, LOGIN_PROBLEMS) },
      ),
    );
  }

  /** Ends the session: the cookie is deleted and a new antiforgery token issued, also without a session. */
  logout(): Promise<void> {
    return lastValueFrom(this.http.post<void>(`${this.apiUrl}/v1/auth/logout`, null));
  }

  /** The legacy user with the UI settings (`GET /api/me`), read while the legacy screens need it. */
  me(): Promise<User | null> {
    return lastValueFrom(this.http.get<User | null>(`${this.apiUrl}/me`));
  }

  /**
   * Registers (R-AUTH-01): 204 for a new and a known address alike, so the answer never tells
   * whether the address has an account. S02 shows field errors, a missing antiforgery token and the
   * rate limit itself ({@link REGISTER_PROBLEMS}).
   */
  register(request: RegistrationRequest): Promise<void> {
    return lastValueFrom(
      this.http.post<void>(`${this.apiUrl}/v1/auth/register`, request, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, REGISTER_PROBLEMS),
      }),
    );
  }

  /**
   * Redeems the token of the confirmation link (R-AUTH-02): 204 confirmed, 400 `validation` for a
   * link that is no longer valid. The confirmation page shows every failure in place.
   */
  confirmEmail(token: string): Promise<void> {
    return lastValueFrom(
      this.http.post<void>(
        `${this.apiUrl}/v1/auth/confirm-email`,
        { token },
        { context: new HttpContext().set(CALLER_SHOWS_FAILURES, true) },
      ),
    );
  }

  /**
   * Sends the confirmation e-mail again (R-AUTH-02), signed in only. A failed send (503
   * `email-delivery`) and the rate limit are shown by the notice ({@link RESEND_PROBLEMS}).
   */
  resendConfirmation(): Promise<void> {
    return lastValueFrom(
      this.http.post<void>(`${this.apiUrl}/v1/auth/confirm-email/resend`, null, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, RESEND_PROBLEMS),
      }),
    );
  }

  /**
   * Opens S01 in the `login` outlet over the current page, which stays (R-AUTH-03); `save` adds
   * `auth.login.reasonSave` (S11 „Liste speichern“, S5-20).
   */
  showLogin(reason?: LoginReason): Promise<boolean> {
    const state: LoginNavigationState | undefined = reason ? { loginReason: reason } : undefined;
    return this.router.navigate([{ outlets: { login: [LOGIN_PATH] } }], { state });
  }

  /** Opens S02 in place of the page and closes S01 (R-AUTH-01). */
  showRegister(): void {
    void this.router.navigate([{ outlets: { primary: [REGISTER_PATH], login: null } }]);
  }

  forgotPassword(email: string): Observable<HttpResponse<unknown>> {
    return this.http.post<unknown>(`${this.apiUrl}/forgot-password`, { email }, { observe: 'response' });
  }

  resetPassword(command: ResetPasswordModel): Observable<{ succeeded: boolean }> {
    return this.http.post<{ succeeded: boolean }>(`${this.apiUrl}/reset-password`, command);
  }
}
