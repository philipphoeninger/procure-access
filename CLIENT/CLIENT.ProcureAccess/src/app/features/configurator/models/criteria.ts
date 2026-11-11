// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { PriorityLevel } from '@app/shared/components/priority-indicator/priority-level';

// The levels live with K5, which shows them outside the configurator too.
export { PRIORITY_LEVELS } from '@app/shared/components/priority-indicator/priority-level';
export type { PriorityLevel } from '@app/shared/components/priority-indicator/priority-level';

/** An app type the release has no requirement criterion for (SPEC §8.2). */
export interface AppTypeWithoutCriteria {
  id: string;
  name: string;
}

/**
 * `requirement`: a criterion of the catalog; `testProcedure`: a „Geprüft nach …“ criterion, one per
 * selected test type, with key `PROC-<testType>`, no number, no product part and no texts (SPEC §8.2).
 */
export type CriterionKind = 'requirement' | 'testProcedure';

/** Which rule of the priority chain gave the level (`criteria.priority.rule.*`, CAT-02). */
export interface MatchedRule {
  kind: 'appTypeAndUseCase' | 'appType' | 'useCase' | 'default';
  appType: string | null;
  useCase: string | null;
}

/**
 * The recommendation of the project team for this selection. It never changes on the client: a
 * user override lives next to it in the list decisions (R-LST-03, CAT-08), so both stay visible.
 */
export interface ResolvedPriority {
  level: PriorityLevel;
  /** Only for B, and only when the rule sets one. */
  weight: number | null;
  rationale: string;
  /** Always `expertJudgement` today („Empfehlung des Projektteams“, CAT-02). */
  source: string;
  matchedRule: MatchedRule;
}

export interface StandardReference {
  standard: 'en301549' | 'wcag' | 'bitInklusiv' | 'iso9241';
  clause: string;
  url: string | null;
}

/** Rating levels a criterion allows; `unknown` is always allowed and never listed (SPEC §8.2). */
export type RatingName =
  | 'fulfilled'
  | 'slightRestriction'
  | 'restriction'
  | 'barrier'
  | 'blockade'
  | 'notApplicable';

/** The BIT inklusiv text of one test procedure, verbatim catalog content (Markdown lines joined). */
export interface ProcedureText {
  testType: string;
  originalNumber: string;
  originalTitle: string;
  applicability: string | null;
  requirement: string | null;
  rationaleWhy: string | null;
  allowedRatings: RatingName[];
  positiveExamples: string | null;
  negativeExamples: string | null;
  sourceUrl: string;
}

/** One criterion of the answer, in the order the server sends (catalog order). */
export interface Criterion {
  revisionId: number;
  key: string;
  kind: CriterionKind;
  /** Null for a „Geprüft nach …“ criterion. */
  number: string | null;
  title: string;
  /** Null while the release has no plain-language title yet (O-1, K-1). */
  plainLanguage: string | null;
  productPart: string | null;
  standardReferences: StandardReference[];
  allowedRatings: RatingName[];
  procedureTexts: ProcedureText[];
  resolvedPriority: ResolvedPriority;
}

/** Answer of `GET /api/v1/catalog/criteria` as the Catalog module builds it (SPEC §8.2, `CriteriaResponse.cs`). */
export interface CriteriaResponse {
  release: string;
  criteria: Criterion[];
  appTypesWithoutCriteria: AppTypeWithoutCriteria[];
}
