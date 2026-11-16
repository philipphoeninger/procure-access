// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';
import { lastValueFrom } from 'rxjs';

import { SavedList, SavedListSummary, SaveListRequest } from '../../models/saved-lists';

/**
 * Client of the configurator endpoints (ARCHITECTURE §7.2): the usage event of S11 under
 * `/api/v1/configurator` and, since S5-20b, the saved lists under `/api/v1/configurations`
 * (SPEC §8.10). The saved-list requests are marked {@link CALLER_SHOWS_FAILURES}: S12, S14 and the
 * page of a saved list show every failure in place.
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

  /** Saves the draft as a list of the organisation (R-SAV-01); resolves to its id. */
  async save(request: SaveListRequest): Promise<string> {
    const created = await lastValueFrom(
      this.http.post<{ id: string }>(`${this.apiUrl}/v1/configurations`, request, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
    return created.id;
  }

  /** A saved list of an organisation the user belongs to; 404 for any other (TEN-01). */
  get(id: string): Promise<SavedList> {
    return lastValueFrom(
      this.http.get<SavedList>(`${this.apiUrl}/v1/configurations/${encodeURIComponent(id)}`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /** The lists of the organisation for S14 (R-SAV-02), the most recently changed first. */
  list(organizationId: string): Promise<SavedListSummary[]> {
    return lastValueFrom(
      this.http.get<SavedListSummary[]>(`${this.apiUrl}/v1/configurations`, {
        params: { organizationId },
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }
}
