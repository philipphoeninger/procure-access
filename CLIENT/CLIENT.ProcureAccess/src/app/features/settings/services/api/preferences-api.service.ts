// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';
import { UserPreferences } from '../../models/user-preferences';

/** HTTP calls of the user's settings (SPEC §8.12; ARCHITECTURE C160). */
@Injectable({ providedIn: 'root' })
export class PreferencesApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

  /**
   * The signed-in user's settings, read after the sign-in and on a page load with a session, not
   * because the user asked: marked {@link BACKGROUND_REQUEST}, so a failure shows nothing and the
   * page keeps following the system.
   */
  get(): Promise<UserPreferences> {
    return lastValueFrom(
      this.http.get<UserPreferences>(`${this.apiUrl}/v1/account/preferences`, {
        context: new HttpContext().set(BACKGROUND_REQUEST, true),
      }),
    );
  }

  /**
   * The settings for S60, read when the page opens: the user asked for the page, so a request
   * without an answer turns the offline notice on; every other failure the page shows in place
   * ({@link CALLER_SHOWS_FAILURES}).
   */
  read(): Promise<UserPreferences> {
    return lastValueFrom(
      this.http.get<UserPreferences>(`${this.apiUrl}/v1/account/preferences`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /**
   * Replaces the settings (`PUT`, SPEC §8.12). Unmarked for S60: a failure shows the system states
   * of the error interceptor — the offline notice, the S80 page or the snackbar of its problem type
   * (owner decision S5-26 D2 (a)). With `background` (S34b remembering its legend, S6-19b) it is a
   * {@link BACKGROUND_REQUEST}: nobody asked to save, so a failure shows nothing.
   */
  save(preferences: UserPreferences, { background = false }: { background?: boolean } = {}): Promise<UserPreferences> {
    return lastValueFrom(
      this.http.put<UserPreferences>(`${this.apiUrl}/v1/account/preferences`, preferences, {
        context: new HttpContext().set(BACKGROUND_REQUEST, background),
      }),
    );
  }
}
