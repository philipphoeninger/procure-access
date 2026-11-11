// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';
import { lastValueFrom } from 'rxjs';

/**
 * Client of the configurator endpoints under `/api/v1/configurator` (ARCHITECTURE §7.2). So far
 * only the usage event of S11; saved lists follow in migration step 3.
 */
@Injectable({ providedIn: 'root' })
export class ConfiguratorApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /**
   * Reports that S11 shows the list of a new complete selection (SPEC §8.7, R-MET-01; owner
   * decision O-S4-48 (a)). No body: the server takes the sign-in from the request. Marked
   * {@link BACKGROUND_REQUEST}, so a failure shows nothing; the caller ignores it.
   */
  reportCriteriaListShown(): Promise<unknown> {
    return lastValueFrom(
      this.http.post(`${this.apiUrl}/v1/configurator/criteria-list-shown`, null, {
        context: new HttpContext().set(BACKGROUND_REQUEST, true),
      }),
    );
  }
}
