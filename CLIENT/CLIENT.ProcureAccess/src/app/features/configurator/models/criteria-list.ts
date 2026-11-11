// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { PriorityLevel } from './criteria';

/**
 * What the user decided for one criterion of S11 (R-LST-03 – R-LST-05). The recommendation stays in
 * the criterion (`resolvedPriority`); this holds only the user's side, so both stay visible (CAT-08).
 */
export interface CriterionDecision {
  /** Excluded criteria stay in the list, marked „Nicht in Ihrer Liste“ (R-LST-04). */
  included: boolean;
  /** Null = the recommended level applies. Never equal to the recommended level. */
  overrideLevel: PriorityLevel | null;
  /**
   * A weight the user set; only while the level that applies is B. Null = the recommended weight,
   * or none when the recommendation is not B (owner decision O-S4-27 (a)).
   */
  overrideWeight: number | null;
  /** As typed; the length is checked against {@link NOTE_MAX_LENGTH}, the server decides (SPEC §7.1). */
  note: string;
}

export const DEFAULT_DECISION: CriterionDecision = {
  included: true,
  overrideLevel: null,
  overrideWeight: null,
  note: '',
};

/**
 * Decisions by criterion key (owner decision O-S4-24 (a)): they stay for the session when the
 * selection changes, also for criteria that leave the list, and apply again when they return.
 * A criterion without an entry has {@link DEFAULT_DECISION}.
 */
export type CriterionDecisions = Readonly<Record<string, CriterionDecision>>;

/** Criterion note, SPEC §7.2 (not a 🔧 value). Counted in UTF-16 code units, as the server's `string.Length`. */
export const NOTE_MAX_LENGTH = 1000;

/** The values of `errors.field.maxLength` („höchstens {{max}} Zeichen. Derzeit sind es {{current}}.“). */
export interface NoteLengthError {
  max: number;
  current: number;
}

/** The range of a user weight (R-LST-03: 1–10, a 🔧 value read from the environment). */
export interface WeightRange {
  min: number;
  max: number;
}

/**
 * The filter of S11 (R-LST-04, R-LST-06). `query` is the applied search text (300 ms after the
 * last keystroke); `levels` empty = every level; the levels compare the level that applies now,
 * override included (owner decision O-S4-25 (a)).
 */
export interface CriteriaListFilter {
  query: string;
  levels: PriorityLevel[];
  onlyIncluded: boolean;
}

export const EMPTY_LIST_FILTER: CriteriaListFilter = {
  query: '',
  levels: [],
  onlyIncluded: false,
};
