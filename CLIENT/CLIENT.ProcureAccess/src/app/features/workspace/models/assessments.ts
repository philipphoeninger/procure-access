// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// The assessment of a candidate per criterion (SPEC R-CAN-06 – R-CAN-08 v2.56; ARCHITECTURE C182;
// S6-18 server, S6-19 client). Types and constants only.

import type { AssessmentValue } from '@app/shared/components/rating-legend/rating-legend';

import type { AssessmentProgress } from './candidates';

/** The wait after the last change of a row before it is saved (R-CAN-06, Handoff §4.6). */
export const AUTOSAVE_DEBOUNCE_MS = 800;

/** `candidate.assessment.saved` is announced at most this often (R-CAN-06, Handoff §4.6). */
export const SAVED_ANNOUNCEMENT_INTERVAL_MS = 5000;

/** The note of a row as the server checks it: at most 1 000 characters after trimming (SPEC §7.2). */
export const ASSESSMENT_NOTE_LIMIT = 1000;

/** A requirement criterion of the project's list with the candidate's assessment (K6 in S34b). */
export interface AssessmentCriterion {
  criterionRevisionId: number;
  /** The EN 301 549 number. */
  number: string | null;
  title: string;
  /** Null until the project team writes it; while there is one, K6 shows it instead of the title. */
  plainLanguageTitle: string | null;
  productPart: string | null;
  /** The levels K6 offers, in scale order; `unknown` is always allowed and not listed (C31). */
  allowedRatings: AssessmentValue[];
  /** `unknown` while the row does not exist (C53). */
  value: AssessmentValue;
  note: string | null;
}

/** `yes` with an evidence of the procedure, `no` when the product was not tested, else `notStated` (CAT-17, C67). */
export type ProcedureAnswerValue = 'yes' | 'no' | 'notStated';

/** A „Geprüft nach …“ criterion of the list: derived from the evidence, never assessed, never counted (R-CAN-07). */
export interface ProcedureAnswer {
  criterionRevisionId: number;
  /** The key of the procedure (`testType.<key>`). */
  testTypeId: string;
  /** The criterion's title, „Geprüft nach …“. */
  title: string;
  answer: ProcedureAnswerValue;
}

/** `GET /api/v1/candidates/{id}/assessments` (SPEC v2.56-1): both lists in list order. */
export interface CandidateAssessments {
  candidateId: string;
  /** Null while the project has no list; then both lists are empty. */
  releaseVersion: string | null;
  /** Shows `candidate.assessment.unionNote` (owner decision S6-18 5 (a)). */
  ratingsFromSeveralProcedures: boolean;
  progress: AssessmentProgress;
  criteria: AssessmentCriterion[];
  procedureAnswers: ProcedureAnswer[];
}

/** Body of `PUT /api/v1/candidates/{id}/assessments/{criterionRevisionId}`: the whole row (SPEC v2.56-2). */
export interface AssessmentRow {
  value: AssessmentValue;
  /** Empty or null for none. */
  note: string | null;
}

/** Whether the note is longer than the server accepts (counted after trimming, as the server does). */
export function noteTooLong(note: string | null): boolean {
  return (note ?? '').trim().length > ASSESSMENT_NOTE_LIMIT;
}
