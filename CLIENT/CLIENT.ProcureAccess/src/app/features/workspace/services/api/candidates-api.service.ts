// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { ifMatch, withEtag } from '@app/core/concurrency/etag';
import { CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { Candidate, CandidateRequest } from '../../models/candidates';

/**
 * Problem types of saving S33 that the form shows itself (SPEC §8.6), as S20 does: field errors,
 * antiforgery and the unconfirmed address. Anything else keeps the system states of the error
 * interceptor (`project-archived` → `projects.archived.note`, 5xx → S80).
 */
export const SAVE_CANDIDATE_PROBLEMS: readonly string[] = ['validation', 'antiforgery', 'email-unconfirmed'];

/**
 * Client of the candidate endpoints of the Workspace module (ARCHITECTURE §7.2; S6-15). S34 shows a
 * failed read as „Seite nicht gefunden“ or the S80 server text in place ({@link CALLER_SHOWS_FAILURES});
 * S33 shows the problems of {@link SAVE_CANDIDATE_PROBLEMS} itself. Evidences and the test status
 * join with S6-17. Every answer with a candidate carries its `ETag`, kept as `etag`; a change sends
 * it back in `If-Match` (API-03, S6-20). A 409 `conflict` shows the general error text until the
 * conflict dialog S37 comes (S6-21).
 */
@Injectable({ providedIn: 'root' })
export class CandidatesApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /** A candidate of a project of an organisation the user belongs to; 404 for any other (TEN-01). */
  get(id: string): Promise<Candidate> {
    return lastValueFrom(
      this.http.get<CandidateBody>(`${this.apiUrl}/v1/candidates/${encodeURIComponent(id)}`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
        observe: 'response',
      }),
    ).then(withEtag);
  }

  /** Adds a candidate to the project (R-CAN-02); Editor or Owner, confirmed address, not archived. */
  create(projectId: string, body: CandidateRequest): Promise<Candidate> {
    return lastValueFrom(
      this.http.post<CandidateBody>(`${this.apiUrl}/v1/projects/${encodeURIComponent(projectId)}/candidates`, body, {
        context: formContext(),
        observe: 'response',
      }),
    ).then(withEtag);
  }

  /**
   * Changes the facts and the private section of a candidate (R-CAN-02); an empty text clears an
   * optional field. `etag` is the tag of the candidate as the form read it (API-03).
   */
  change(id: string, body: CandidateRequest, etag: string): Promise<Candidate> {
    return lastValueFrom(
      this.http.patch<CandidateBody>(`${this.apiUrl}/v1/candidates/${encodeURIComponent(id)}`, body, {
        context: formContext(),
        headers: ifMatch(etag),
        observe: 'response',
      }),
    ).then(withEtag);
  }
}

/** A candidate as the body of an answer names it; its tag is the `ETag` header. */
type CandidateBody = Omit<Candidate, 'etag'>;

function formContext(): HttpContext {
  return new HttpContext().set(CALLER_HANDLES_PROBLEMS, SAVE_CANDIDATE_PROBLEMS);
}
