// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { ifMatch, withEtag } from '@app/core/concurrency/etag';
import { CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { AssessmentRow, CandidateAssessments } from '../../models/assessments';
import { Candidate, CandidateRequest, TestEvidence, TestEvidenceRequest, TestStatus } from '../../models/candidates';

/**
 * Problem types of saving S33 that the form shows itself (SPEC §8.6), as S20 does: field errors,
 * antiforgery, the unconfirmed address and `conflict` — a stale `If-Match` opens S37 (R-CAN-11,
 * S6-21; adding needs no tag and never meets it). Anything else keeps the system states of the
 * error interceptor (`project-archived` → `projects.archived.note`, 5xx → S80).
 */
export const SAVE_CANDIDATE_PROBLEMS: readonly string[] = ['validation', 'antiforgery', 'email-unconfirmed', 'conflict'];

/**
 * Problem types of changing the test status that S34a shows itself: those of
 * {@link SAVE_CANDIDATE_PROBLEMS} and `test-status-conflict` — leaving „Ja“ while evidences exist
 * asks `candidate.evidence.list.statusChange` and repeats the change (SPEC §8.6 v2.53, WSP-01).
 * The evidence requests keep the interceptor's general text for it: they meet it only when another
 * session changed the status meanwhile.
 */
export const SAVE_TEST_STATUS_PROBLEMS: readonly string[] = [...SAVE_CANDIDATE_PROBLEMS, 'test-status-conflict'];

/**
 * Client of the candidate endpoints of the Workspace module (ARCHITECTURE §7.2; S6-15). S34 shows a
 * failed read as „Seite nicht gefunden“ or the S80 server text in place ({@link CALLER_SHOWS_FAILURES});
 * S33 and S34a show the problems of {@link SAVE_CANDIDATE_PROBLEMS} themselves; S34a also
 * `test-status-conflict` of the test status (S6-17). Every answer with a candidate carries its
 * `ETag`, kept as `etag`; an evidence names its tag as the body member `etag`. A change sends the
 * tag back in `If-Match` (API-03, S6-20): the candidate's for its facts and its test status, the
 * evidence's for changing or removing it; adding one needs none. A 409 `conflict` is the caller's:
 * S33 and S34a open the conflict dialog S37 (S6-21).
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

  /**
   * Sets the test status (R-CAN-05); Editor or Owner, confirmed address, not archived. Leaving
   * „Ja“ with evidences answers 409 `test-status-conflict` unless `removeEvidences` asks to remove
   * them with the change (WSP-01). `etag` is the candidate's tag: an evidence added or removed
   * since changes it, so a status change never removes evidences the user has not seen (S6-20).
   */
  changeTestStatus(id: string, testStatus: TestStatus, removeEvidences: boolean, etag: string): Promise<Candidate> {
    return lastValueFrom(
      this.http.put<CandidateBody>(
        `${this.apiUrl}/v1/candidates/${encodeURIComponent(id)}/test-status`,
        { testStatus, removeEvidences },
        { context: formContext(SAVE_TEST_STATUS_PROBLEMS), headers: ifMatch(etag), observe: 'response' },
      ),
    ).then(withEtag);
  }

  /** Adds an evidence while the status is „Ja“; a second one of a catalog procedure is 400 at `testTypeId` (WSP-01). */
  addEvidence(candidateId: string, body: TestEvidenceRequest): Promise<TestEvidence> {
    return lastValueFrom(
      this.http.post<TestEvidence>(`${this.apiUrl}/v1/candidates/${encodeURIComponent(candidateId)}/test-evidences`, body, {
        context: formContext(),
      }),
    );
  }

  /**
   * Replaces the fields of an evidence — the same row for a new product version (WSP-01, C65).
   * `etag` is the evidence's own tag (its body member).
   */
  changeEvidence(id: string, body: TestEvidenceRequest, etag: string): Promise<TestEvidence> {
    return lastValueFrom(
      this.http.put<TestEvidence>(`${this.apiUrl}/v1/test-evidences/${encodeURIComponent(id)}`, body, {
        context: formContext(),
        headers: ifMatch(etag),
      }),
    );
  }

  /**
   * The assessment tab S34b of a candidate (R-CAN-06 – R-CAN-08; SPEC v2.56-1): the included
   * requirement criteria of the project's list with their values, and the derived „Geprüft nach …“
   * rows. S34b shows a failure itself ({@link CALLER_SHOWS_FAILURES}).
   */
  assessments(candidateId: string): Promise<CandidateAssessments> {
    return lastValueFrom(
      this.http.get<CandidateAssessments>(`${this.apiUrl}/v1/candidates/${encodeURIComponent(candidateId)}/assessments`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /**
   * Stores the whole row of one criterion (SPEC v2.56-2): the last write wins, no `If-Match`
   * (R-CAN-11). Autosaved, so a failure keeps the error interceptor's system states (the general
   * text, `project-archived`, `email-unconfirmed`); the answer is not needed.
   */
  async saveAssessment(candidateId: string, criterionRevisionId: number, row: AssessmentRow): Promise<void> {
    await lastValueFrom(
      this.http.put<unknown>(`${this.apiUrl}/v1/candidates/${encodeURIComponent(candidateId)}/assessments/${criterionRevisionId}`, {
        value: row.value,
        note: row.note === null || row.note.trim() === '' ? null : row.note,
      }),
    );
  }

  /**
   * Removes an evidence with its private note. S34a also handles `not-found` itself: an evidence
   * another session removed meanwhile is gone, as asked. `etag` is the evidence's own tag.
   */
  removeEvidence(id: string, etag: string): Promise<void> {
    return lastValueFrom(
      this.http.delete<void>(`${this.apiUrl}/v1/test-evidences/${encodeURIComponent(id)}`, {
        context: formContext([...SAVE_CANDIDATE_PROBLEMS, 'not-found']),
        headers: ifMatch(etag),
      }),
    );
  }
}

/** A candidate as the body of an answer names it; its tag is the `ETag` header. */
type CandidateBody = Omit<Candidate, 'etag'>;

function formContext(problems: readonly string[] = SAVE_CANDIDATE_PROBLEMS): HttpContext {
  return new HttpContext().set(CALLER_HANDLES_PROBLEMS, problems);
}
