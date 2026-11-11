// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Criterion, PriorityLevel } from '../models/criteria';
import {
  CriteriaListFilter,
  CriterionDecision,
  CriterionDecisions,
  DEFAULT_DECISION,
  NOTE_MAX_LENGTH,
  NoteLengthError,
  WeightRange,
} from '../models/criteria-list';

/**
 * Rules of the criteria list S11 (R-LST-03 – R-LST-06), as pure functions.
 *
 * Every change returns new decisions, or the same object when nothing changes. The recommendation
 * in `criterion.resolvedPriority` is never touched: an override lives next to it (CAT-08). A
 * decision equal to {@link DEFAULT_DECISION} is removed, so the map holds only what the user changed.
 */

export function decisionOf(decisions: CriterionDecisions, key: string): CriterionDecision {
  return decisions[key] ?? DEFAULT_DECISION;
}

/** The level that applies: the user's override, else the recommendation. */
export function effectiveLevel(criterion: Criterion, decision: CriterionDecision): PriorityLevel {
  return decision.overrideLevel ?? criterion.resolvedPriority.level;
}

/**
 * The weight that applies, only for B: the user's weight, else the recommended one when the
 * recommendation is B too. An override from A or Info to B has no weight until the user sets one
 * (owner decision O-S4-27 (a)).
 */
export function effectiveWeight(criterion: Criterion, decision: CriterionDecision): number | null {
  if (effectiveLevel(criterion, decision) !== 'B') {
    return null;
  }
  const recommended = criterion.resolvedPriority;
  return decision.overrideWeight ?? (recommended.level === 'B' ? recommended.weight : null);
}

/** True when the user changed the level or the weight („Empfehlung wiederherstellen“ applies). */
export function hasPriorityOverride(decision: CriterionDecision): boolean {
  return decision.overrideLevel !== null || decision.overrideWeight !== null;
}

/** Includes or excludes a criterion; an excluded one stays in the list (R-LST-04). */
export function setIncluded(decisions: CriterionDecisions, key: string, included: boolean): CriterionDecisions {
  const decision = decisionOf(decisions, key);
  return decision.included === included ? decisions : withDecision(decisions, key, { ...decision, included });
}

/**
 * Sets the level of a criterion (R-LST-03). Choosing the recommended level removes the override,
 * and a level other than B drops the user's weight. The recommendation stays as it is.
 */
export function overridePriority(
  decisions: CriterionDecisions,
  criterion: Criterion,
  level: PriorityLevel,
): CriterionDecisions {
  const decision = decisionOf(decisions, criterion.key);
  const overrideLevel = level === criterion.resolvedPriority.level ? null : level;
  const overrideWeight = level === 'B' ? decision.overrideWeight : null;
  if (overrideLevel === decision.overrideLevel && overrideWeight === decision.overrideWeight) {
    return decisions;
  }
  return withDecision(decisions, criterion.key, { ...decision, overrideLevel, overrideWeight });
}

/** True for a whole number inside the configured range (🔧, {@link WeightRange}). */
export function isWeightInRange(weight: number, range: WeightRange): boolean {
  return Number.isInteger(weight) && weight >= range.min && weight <= range.max;
}

/**
 * Sets the weight of a criterion whose level is B now. A weight outside the range, or for a level
 * other than B, changes nothing; the recommended weight itself removes the user's weight.
 */
export function setWeight(
  decisions: CriterionDecisions,
  criterion: Criterion,
  weight: number,
  range: WeightRange,
): CriterionDecisions {
  const decision = decisionOf(decisions, criterion.key);
  if (effectiveLevel(criterion, decision) !== 'B' || !isWeightInRange(weight, range)) {
    return decisions;
  }
  const recommended = criterion.resolvedPriority;
  const overrideWeight =
    decision.overrideLevel === null && recommended.level === 'B' && recommended.weight === weight ? null : weight;
  return overrideWeight === decision.overrideWeight
    ? decisions
    : withDecision(decisions, criterion.key, { ...decision, overrideWeight });
}

/** „Empfehlung wiederherstellen“: level and weight follow the recommendation again (R-LST-03). */
export function resetPriority(decisions: CriterionDecisions, key: string): CriterionDecisions {
  const decision = decisionOf(decisions, key);
  return hasPriorityOverride(decision)
    ? withDecision(decisions, key, { ...decision, overrideLevel: null, overrideWeight: null })
    : decisions;
}

/**
 * Keeps the note as typed, also when it is too long: the field shows the length error on submit
 * (SPEC §7.1) with the text still there to shorten (R-LST-05).
 */
export function setNote(decisions: CriterionDecisions, key: string, note: string): CriterionDecisions {
  const decision = decisionOf(decisions, key);
  return decision.note === note ? decisions : withDecision(decisions, key, { ...decision, note });
}

/** The values of `errors.field.maxLength` when the note is longer than SPEC §7.2 allows, else null. */
export function noteLengthError(note: string): NoteLengthError | null {
  return note.length > NOTE_MAX_LENGTH ? { max: NOTE_MAX_LENGTH, current: note.length } : null;
}

/**
 * Search over what the compact card shows (owner decision O-S4-26 (a)): key and number, title,
 * plain-language title and the original titles of the procedure texts. Case-insensitive substring
 * of the whole trimmed query; umlauts are not folded. An empty query matches every criterion.
 */
export function matchesSearch(criterion: Criterion, query: string): boolean {
  const needle = query.trim().toLocaleLowerCase('de');
  if (needle === '') {
    return true;
  }
  const fields = [
    criterion.key,
    criterion.number,
    criterion.title,
    criterion.plainLanguage,
    ...criterion.procedureTexts.map((t) => t.originalTitle),
  ];
  return fields.some((field) => field?.toLocaleLowerCase('de').includes(needle) ?? false);
}

/** True when any part of the filter hides criteria. */
export function isFilterActive(filter: CriteriaListFilter): boolean {
  return filter.query.trim() !== '' || filter.levels.length > 0 || filter.onlyIncluded;
}

/**
 * The criteria the filter shows, in the order of the answer (R-LST-04, R-LST-06): search, the
 * level that applies now (owner decision O-S4-25 (a)) and „Nur aufgenommene Kriterien anzeigen“.
 */
export function filterCriteria(
  criteria: readonly Criterion[],
  decisions: CriterionDecisions,
  filter: CriteriaListFilter,
): Criterion[] {
  return criteria.filter((criterion) => {
    const decision = decisionOf(decisions, criterion.key);
    return (
      matchesSearch(criterion, filter.query) &&
      (filter.levels.length === 0 || filter.levels.includes(effectiveLevel(criterion, decision))) &&
      (!filter.onlyIncluded || decision.included)
    );
  });
}

function withDecision(decisions: CriterionDecisions, key: string, decision: CriterionDecision): CriterionDecisions {
  const next = { ...decisions };
  if (isDefault(decision)) {
    delete next[key];
  } else {
    next[key] = decision;
  }
  return next;
}

function isDefault(decision: CriterionDecision): boolean {
  return (
    decision.included === DEFAULT_DECISION.included &&
    decision.overrideLevel === DEFAULT_DECISION.overrideLevel &&
    decision.overrideWeight === DEFAULT_DECISION.overrideWeight &&
    decision.note === DEFAULT_DECISION.note
  );
}
