// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_URL } from '@app/app.config';
import { lastValueFrom } from 'rxjs';

import { CatalogSelection } from '../../models/configurator-draft';
import { FilterOptionsResponse } from '../../models/filter-options';

/**
 * Client of the catalog endpoints under `/api/v1/catalog` (ARCHITECTURE §7.2, SPEC §8.1).
 * The answers are plain JSON without the legacy `TResult` wrapper; errors are Problem Details
 * and reach the caller as `HttpErrorResponse` (the error interceptor handles 5xx and offline).
 */
@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /**
   * Filter options of the release named by `release`, or of the active one without it, with the
   * `excludedBy[]` of each option under the given selection.
   */
  getFilterOptions(selection: CatalogSelection, release?: string): Promise<FilterOptionsResponse> {
    let params = new HttpParams();
    if (release !== undefined) {
      params = params.set('release', release);
    }
    if (selection.useCase !== undefined) {
      params = params.set('useCase', selection.useCase);
    }
    for (const id of selection.appTypeIds) {
      params = params.append('appTypeIds[]', id);
    }
    for (const id of selection.productPartIds) {
      params = params.append('productPartIds[]', id);
    }
    return lastValueFrom(
      this.http.get<FilterOptionsResponse>(`${this.apiUrl}/v1/catalog/filter-options`, { params }),
    );
  }
}
