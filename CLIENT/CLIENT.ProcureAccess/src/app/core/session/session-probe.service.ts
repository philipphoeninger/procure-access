// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { EnvironmentProviders, inject, Injectable, provideAppInitializer } from '@angular/core';
import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';
import { lastValueFrom } from 'rxjs';

/**
 * The session probe `GET /api/v1/auth/session` (ARCHITECTURE §8.3, SPEC §8.8; S5-07, owner
 * decision 2a). It answers 200 for everyone and issues the antiforgery cookie `XSRF-TOKEN`;
 * Angular's built-in XSRF interceptor then sends its value as `X-XSRF-TOKEN` with every
 * state-changing request to a relative URL, so `POST /api/v1/configurator/criteria-list-shown`
 * passes the server's antiforgery check. The answer itself (who is signed in, when the session
 * ends) is read from S5-08 on, when `withIdentity()` replaces the legacy JWT.
 */
@Injectable({ providedIn: 'root' })
export class SessionProbeService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /**
   * Calls the probe once. Marked {@link BACKGROUND_REQUEST}: the user did not ask for it, so a
   * failure shows nothing, and the returned promise never rejects — the app starts either way.
   */
  async probe(): Promise<void> {
    try {
      await lastValueFrom(
        this.http.get<unknown>(`${this.apiUrl}/v1/auth/session`, {
          context: new HttpContext().set(BACKGROUND_REQUEST, true),
        }),
      );
    } catch {
      // A probe that fails leaves the client without a token; the next state-changing request
      // answers 400 antiforgery, which its caller handles like any other failure.
    }
  }
}

/**
 * Calls the session probe at application start (ARCHITECTURE §8.3: "At application start the shell
 * calls `GET /api/v1/auth/session`"). Fire-and-forget: the start does not wait for the answer.
 */
export function provideSessionProbe(): EnvironmentProviders {
  return provideAppInitializer(() => {
    void inject(SessionProbeService).probe();
  });
}
