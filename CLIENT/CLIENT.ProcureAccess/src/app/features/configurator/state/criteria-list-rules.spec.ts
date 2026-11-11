// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Criterion } from '../models/criteria';
import { CriterionDecisions, DEFAULT_DECISION, EMPTY_LIST_FILTER, WeightRange } from '../models/criteria-list';
import { criteriaAnswer, criterion } from '../testing/criteria-fixture';
import {
  decisionOf,
  effectiveLevel,
  effectiveWeight,
  filterCriteria,
  hasPriorityOverride,
  includedCount,
  isFilterActive,
  isWeightInRange,
  levelCounts,
  matchesSearch,
  noteLengthError,
  overridePriority,
  resetPriority,
  sameSelection,
  setIncluded,
  setNote,
  setWeight,
} from './criteria-list-rules';

const range: WeightRange = { min: 1, max: 10 };
const none: CriterionDecisions = {};

describe('criteria list rules', () => {
  const a = criterion('11.1.1', 'A', null);
  const b = criterion('11.2.2', 'B', 3);

  describe('decisions', () => {
    it('starts every criterion included, without override and note', () => {
      expect(decisionOf(none, 'any')).toEqual({ included: true, overrideLevel: null, overrideWeight: null, note: '' });
    });

    it('excludes and includes again, removing the entry once it is the default', () => {
      const excluded = setIncluded(none, a.key, false);
      expect(decisionOf(excluded, a.key).included).toBe(false);

      expect(setIncluded(excluded, a.key, true)).toEqual({});
    });

    it('returns the same object when nothing changes', () => {
      expect(setIncluded(none, a.key, true)).toBe(none);
      expect(overridePriority(none, a, 'A')).toBe(none);
      expect(resetPriority(none, a.key)).toBe(none);
      expect(setNote(none, a.key, '')).toBe(none);
    });
  });

  describe('priority override (R-LST-03)', () => {
    it('keeps the recommended level unchanged next to an override', () => {
      const decisions = overridePriority(none, a, 'B');

      expect(effectiveLevel(a, decisionOf(decisions, a.key))).toBe('B');
      expect(a.resolvedPriority.level).toBe('A');
      expect(decisionOf(decisions, a.key).overrideLevel).toBe('B');
      expect(hasPriorityOverride(decisionOf(decisions, a.key))).toBe(true);
    });

    it('treats the recommended level as reset', () => {
      const overridden = overridePriority(none, a, 'Info');

      expect(overridePriority(overridden, a, 'A')).toEqual({});
    });

    it('gives an override from A to B no weight until the user sets one (O-S4-27 (a))', () => {
      const decisions = overridePriority(none, a, 'B');

      expect(effectiveWeight(a, decisionOf(decisions, a.key))).toBeNull();

      const weighted = setWeight(decisions, a, 7, range);
      expect(effectiveWeight(a, decisionOf(weighted, a.key))).toBe(7);
    });

    it('shows the recommended weight of a B criterion until the user changes it', () => {
      expect(effectiveWeight(b, DEFAULT_DECISION)).toBe(3);

      const decisions = setWeight(none, b, 8, range);
      expect(effectiveWeight(b, decisionOf(decisions, b.key))).toBe(8);
      expect(b.resolvedPriority.weight).toBe(3);
    });

    it('drops the user weight when the level stops being B', () => {
      const weighted = setWeight(none, b, 8, range);
      const asA = overridePriority(weighted, b, 'A');

      expect(decisionOf(asA, b.key).overrideWeight).toBeNull();
      expect(effectiveWeight(b, decisionOf(asA, b.key))).toBeNull();

      const backToB = overridePriority(asA, b, 'B');
      expect(backToB).toEqual({});
      expect(effectiveWeight(b, decisionOf(backToB, b.key))).toBe(3);
    });

    it('refuses a weight outside the range, not whole, or for a level other than B', () => {
      expect(setWeight(none, b, 0, range)).toBe(none);
      expect(setWeight(none, b, 11, range)).toBe(none);
      expect(setWeight(none, b, 2.5, range)).toBe(none);
      expect(setWeight(none, a, 5, range)).toBe(none);
      expect(isWeightInRange(1, range)).toBe(true);
      expect(isWeightInRange(10, range)).toBe(true);
    });

    it('reads the range it is given, not a fixed 1–10', () => {
      expect(setWeight(none, b, 12, { min: 1, max: 12 })).not.toBe(none);
    });

    it('removes the user weight when it equals the recommended one', () => {
      const weighted = setWeight(none, b, 8, range);

      expect(setWeight(weighted, b, 3, range)).toEqual({});
    });

    it('resets level and weight but keeps inclusion and note', () => {
      let decisions = setIncluded(none, b.key, false);
      decisions = setNote(decisions, b.key, 'Wichtig');
      decisions = overridePriority(decisions, b, 'B');
      decisions = setWeight(decisions, b, 9, range);
      decisions = overridePriority(decisions, b, 'A');

      const reset = resetPriority(decisions, b.key);

      expect(decisionOf(reset, b.key)).toEqual({ included: false, overrideLevel: null, overrideWeight: null, note: 'Wichtig' });
    });
  });

  describe('note (R-LST-05)', () => {
    it('keeps a note as typed, also when it is too long', () => {
      const long = 'x'.repeat(1001);

      expect(decisionOf(setNote(none, a.key, long), a.key).note).toBe(long);
    });

    it('reports the length error with the values of errors.field.maxLength', () => {
      expect(noteLengthError('x'.repeat(1000))).toBeNull();
      expect(noteLengthError('x'.repeat(1001))).toEqual({ max: 1000, current: 1001 });
    });

    it('counts UTF-16 code units, as the server does', () => {
      expect(noteLengthError('😀'.repeat(501))).toEqual({ max: 1000, current: 1002 });
    });
  });

  describe('search (R-LST-06, O-S4-26 (a))', () => {
    const [nonText, keyboard, headings, procedure] = criteriaAnswer().criteria as [Criterion, Criterion, Criterion, Criterion];

    it('matches number, title, plain-language title and the original titles of the procedure texts', () => {
      expect(matchesSearch(nonText, '11.1')).toBe(true);
      expect(matchesSearch(nonText, 'nicht-text')).toBe(true);
      expect(matchesSearch(nonText, 'ALTERNATIVTEXT')).toBe(true);
      expect(matchesSearch(nonText, 'inhalte prüfen')).toBe(true);
      expect(matchesSearch(keyboard, 'nicht-text')).toBe(false);
    });

    it('does not search the long procedure texts', () => {
      expect(matchesSearch(nonText, 'requirement text')).toBe(false);
    });

    it('matches a criterion without plain-language title and a „Geprüft nach …“ criterion', () => {
      expect(matchesSearch(headings, 'überschrift')).toBe(true);
      expect(matchesSearch(procedure, 'softwaretest')).toBe(true);
    });

    it('trims the query, matches everything when it is empty, and does not fold umlauts', () => {
      expect(matchesSearch(keyboard, '  tastatur ')).toBe(true);
      expect(matchesSearch(keyboard, '   ')).toBe(true);
      expect(matchesSearch(headings, 'uberschrift')).toBe(false);
    });
  });

  describe('filter (R-LST-04, R-LST-06)', () => {
    const criteria = criteriaAnswer().criteria;
    const keys = (list: Criterion[]) => list.map((c) => c.key);

    it('shows every criterion in the order of the answer without a filter', () => {
      expect(keys(filterCriteria(criteria, none, EMPTY_LIST_FILTER))).toEqual([
        '11.1.1',
        '11.2.2',
        '11.3.3',
        'PROC-softwareTest',
      ]);
      expect(isFilterActive(EMPTY_LIST_FILTER)).toBe(false);
    });

    it('filters by the level that applies now, override included (O-S4-25 (a))', () => {
      const decisions = overridePriority(none, criteria[0], 'B');
      const filter = { ...EMPTY_LIST_FILTER, levels: ['B' as const] };

      expect(keys(filterCriteria(criteria, decisions, filter))).toEqual(['11.1.1', '11.2.2', 'PROC-softwareTest']);
      expect(isFilterActive(filter)).toBe(true);
    });

    it('accepts several levels', () => {
      const filter = { ...EMPTY_LIST_FILTER, levels: ['A' as const, 'Info' as const] };

      expect(keys(filterCriteria(criteria, none, filter))).toEqual(['11.1.1', '11.3.3']);
    });

    it('shows only included criteria when asked', () => {
      const decisions = setIncluded(none, 'PROC-softwareTest', false);
      const filter = { ...EMPTY_LIST_FILTER, onlyIncluded: true };

      expect(keys(filterCriteria(criteria, decisions, filter))).toEqual(['11.1.1', '11.2.2', '11.3.3']);
    });

    it('combines search, level and inclusion', () => {
      const decisions = setIncluded(none, '11.2.2', false);
      const filter = { query: '11', levels: ['B' as const], onlyIncluded: true };

      expect(filterCriteria(criteria, decisions, filter)).toEqual([]);
    });
  });

  describe('selection of a list (owner decision O-S4-46 (a))', () => {
    const selection = { useCase: 'hr', appTypeIds: ['web', 'document'], productPartIds: ['base'], testTypeIds: [] };

    it('treats the same values in another order as the same selection', () => {
      expect(sameSelection(selection, { ...selection, appTypeIds: ['document', 'web'] })).toBe(true);
    });

    it('tells a changed use case or a changed layer apart', () => {
      expect(sameSelection(selection, { ...selection, useCase: undefined })).toBe(false);
      expect(sameSelection(selection, { ...selection, appTypeIds: ['web'] })).toBe(false);
      expect(sameSelection(selection, { ...selection, productPartIds: ['base', 'audio'] })).toBe(false);
      expect(sameSelection(selection, { ...selection, testTypeIds: ['softwareTest'] })).toBe(false);
    });
  });

  describe('levelCounts and includedCount (S4-18a, owner decision O-S4-51 (a))', () => {
    const criteria = criteriaAnswer().criteria;

    it('counts the requirement criteria by recommended level, without „Geprüft nach …“', () => {
      expect(levelCounts(criteria, none)).toEqual({ total: 3, A: 1, B: 1, Info: 1 });
    });

    it('counts the level that applies now, also for an excluded criterion', () => {
      const decisions: CriterionDecisions = {
        '11.3.3': { ...DEFAULT_DECISION, overrideLevel: 'A' },
        '11.2.2': { ...DEFAULT_DECISION, included: false },
      };

      expect(levelCounts(criteria, decisions)).toEqual({ total: 3, A: 2, B: 1, Info: 0 });
    });

    it('counts every included card, „Geprüft nach …“ too', () => {
      expect(includedCount(criteria, none)).toBe(4);
      expect(
        includedCount(criteria, {
          'PROC-softwareTest': { ...DEFAULT_DECISION, included: false },
          '11.1.1': { ...DEFAULT_DECISION, included: false },
        }),
      ).toBe(2);
    });
  });
});
