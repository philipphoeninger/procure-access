// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_URL } from '@app/app.config';
import { CALLER_HANDLES_PROBLEMS } from '@app/core/errors/problem-details';
import { lastValueFrom } from 'rxjs';

import { CatalogSelection, CriteriaSelection } from '../../models/configurator-draft';
import { CriteriaResponse } from '../../models/criteria';
import { FilterOptionsResponse } from '../../models/filter-options';

/** Problem types „Kriterienliste anzeigen“ shows at the step of S10 (R-CON-10). */
export const SELECTION_CHECK_PROBLEMS: readonly string[] = ['validation', 'selection-excluded', 'not-found'];

/**
 * Problem types the list S11 shows itself: only `not-found`, which opens the legacy filter. A 400
 * for a selection S10 had sent successfully gets the generic snackbar (owner decision O-S4-34 (a)).
 */
export const CRITERIA_LIST_PROBLEMS: readonly string[] = ['not-found'];

/**
 * Client of the catalog endpoints under `/api/v1/catalog` (ARCHITECTURE §7.2, SPEC §8.1, §8.2).
 * The answers are plain JSON without the legacy `TResult` wrapper; errors are Problem Details
 * and reach the caller as `HttpErrorResponse` (the error interceptor handles 5xx and offline).
 * A 404 means that no release is active or the pinned one is unknown; the configurator handles it
 * (O-S4-3), so these requests get no error snackbar for it.
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
    return lastValueFrom(
      this.http.get<FilterOptionsResponse>(`${this.apiUrl}/v1/catalog/filter-options`, {
        params: selectionParams(selection, release),
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, ['not-found']),
      }),
    );
  }

  /**
   * Criteria of a complete selection with their resolved priorities. The server checks the
   * selection again (R-CON-10, CAT-12): 400 `validation` or `selection-excluded` with field errors
   * as sent (`appTypeIds[1]`), 404 `not-found`. `callerHandles` names the problem types the caller
   * shows itself: S10 all three ({@link SELECTION_CHECK_PROBLEMS}), S11 only `not-found`
   * ({@link CRITERIA_LIST_PROBLEMS}).
   */
  getCriteria(
    selection: CriteriaSelection,
    release?: string,
    callerHandles: readonly string[] = SELECTION_CHECK_PROBLEMS,
  ): Promise<CriteriaResponse> {
    let params = selectionParams(selection, release);
    for (const id of selection.testTypeIds) {
      params = params.append('testTypeIds[]', id);
    }
    return lastValueFrom(
      this.http.get<CriteriaResponse>(`${this.apiUrl}/v1/catalog/criteria`, {
        params,
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, callerHandles),
      }),
    );
  }
}

/** Query parameters of both catalog endpoints, named as in SPEC §8.1. */
function selectionParams(selection: CatalogSelection, release: string | undefined): HttpParams {
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
  return params;
}
