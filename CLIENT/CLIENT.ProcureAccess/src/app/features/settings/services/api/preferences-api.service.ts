// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';
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
}
