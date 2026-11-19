// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_URL } from '@app/app.config';
import { ifMatch, withEtag } from '@app/core/concurrency/etag';
import { BACKGROUND_REQUEST, CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';
import { lastValueFrom } from 'rxjs';

import { ConfiguratorSettingsResponse } from '../../models/configurator-settings';
import {
  ChangedSelection,
  CopiedList,
  SavedList,
  SavedListDecisionRequest,
  SavedListSelectionRequest,
  SavedListSummary,
  SaveListRequest,
} from '../../models/saved-lists';
import { CreatedShareLink, SharedList, ShareLinksResponse } from '../../models/share-links';

/**
 * Client of the configurator endpoints (ARCHITECTURE §7.2): the usage event of S11 and, since
 * S5-22, the settings (SPEC §8.11) under `/api/v1/configurator` and, since S5-20b, the saved lists under `/api/v1/configurations`
 * (SPEC §8.10). The saved-list requests are marked {@link CALLER_SHOWS_FAILURES}: S12, S14 and the
 * page of a saved list show every failure in place — except the changes of a list (a decision, the
 * selection, deleting, the copy), whose failure the error interceptor shows as everywhere (owner
 * decision S5-21b 1a). Since S5-24 also the share links of a list for S15 and the shared list of
 * S16 (`/api/v1/shared/{token}`, SPEC §8.5), both marked {@link CALLER_SHOWS_FAILURES}: S15 and S16
 * show every failure in place.
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

  /**
   * The settings the server decides (SPEC §8.11): the range of a user weight. Not marked, so a
   * failure shows the system state (offline notice or server page) like any request.
   */
  settings(): Promise<ConfiguratorSettingsResponse> {
    return lastValueFrom(this.http.get<ConfiguratorSettingsResponse>(`${this.apiUrl}/v1/configurator/settings`));
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

  /**
   * A saved list of an organisation the user belongs to, with the `ETag` of the answer as `etag`
   * (API-03, S6-20b); 404 for any other (TEN-01).
   */
  get(id: string): Promise<SavedList> {
    return lastValueFrom(
      this.http.get<Omit<SavedList, 'etag'>>(`${this.apiUrl}/v1/configurations/${encodeURIComponent(id)}`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
        observe: 'response',
      }),
    ).then(withEtag);
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

  /**
   * Saves the decision on one criterion of a saved list (R-SAV-03) as a change of the list read at
   * `etag`, and resolves to the list's new tag for the next change (API-03, S6-20b). A 409
   * `conflict` is the caller's (S11 opens S37, S6-21b); any other failure shows the general error.
   */
  async decide(id: string, criterionRevisionId: number, decision: SavedListDecisionRequest, etag: string): Promise<string> {
    const response = await lastValueFrom(
      this.http.patch(`${this.listUrl(id)}/criteria/${criterionRevisionId}`, decision, {
        context: conflictContext(),
        headers: ifMatch(etag),
        observe: 'response',
      }),
    );
    const next = response.headers.get('ETag');
    if (next === null) throw new Error('The answer of a saved decision has no ETag.');
    return next;
  }

  /**
   * „Auswahl ändern“ (R-SAV-03): the new selection under the list's release, for the list read at
   * `etag` (API-03). A 409 `conflict` is the caller's (S10 opens S37, S6-21b).
   */
  changeSelection(id: string, request: SavedListSelectionRequest, etag: string): Promise<ChangedSelection> {
    return lastValueFrom(
      this.http.put<ChangedSelection>(`${this.listUrl(id)}/selection`, request, { context: conflictContext(), headers: ifMatch(etag) }),
    );
  }

  /** Deletes a saved list as it was read at `etag` (R-SAV-06, API-03); a failure shows the general error. */
  delete(id: string, etag: string): Promise<unknown> {
    return lastValueFrom(this.http.delete(this.listUrl(id), { headers: ifMatch(etag) }));
  }

  /** „Neue Liste mit derselben Auswahl“ (R-LST-14, CAT-22): a copy on the active release. */
  copyToActiveRelease(id: string): Promise<CopiedList> {
    return lastValueFrom(this.http.post<CopiedList>(`${this.listUrl(id)}/copies`, { release: 'active' }));
  }

  /** S15: the dates a new link may end on today and the active links of a list (R-SAV-04; owner decision S5-24 1a). */
  shareLinks(id: string): Promise<ShareLinksResponse> {
    return lastValueFrom(
      this.http.get<ShareLinksResponse>(`${this.listUrl(id)}/share-links`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /** S15 „Link erstellen“ (R-SAV-04): a link that works until the end of `expiresOn` (`yyyy-MM-dd`). */
  createShareLink(id: string, expiresOn: string): Promise<CreatedShareLink> {
    return lastValueFrom(
      this.http.post<CreatedShareLink>(
        `${this.listUrl(id)}/share-links`,
        { expiresOn },
        { context: new HttpContext().set(CALLER_SHOWS_FAILURES, true) },
      ),
    );
  }

  /** S15 „Link ungültig machen“ (R-SAV-04, US-SAV-03). */
  revokeShareLink(linkId: string): Promise<unknown> {
    return lastValueFrom(
      this.http.delete(`${this.apiUrl}/v1/share-links/${encodeURIComponent(linkId)}`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /** S16: the list behind a share link, without an account (R-SAV-05, TEN-09); 404 for a link that no longer works. */
  shared(token: string): Promise<SharedList> {
    return lastValueFrom(
      this.http.get<SharedList>(`${this.apiUrl}/v1/shared/${encodeURIComponent(token)}`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  private listUrl(id: string): string {
    return `${this.apiUrl}/v1/configurations/${encodeURIComponent(id)}`;
  }
}

/** A change of a saved list whose caller shows a 409 `conflict` in S37 (R-CAN-11, S6-21b). */
function conflictContext(): HttpContext {
  return new HttpContext().set(CALLER_HANDLES_PROBLEMS, ['conflict']);
}
