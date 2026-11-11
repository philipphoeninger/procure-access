// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { describe, expect, it } from 'vitest';

import { ProcedureText } from '../models/criteria';
import { criterion, procedureText } from '../testing/criteria-fixture';
import { detailLayout, ratingsOf, tabLabelKey } from './criterion-detail-rules';

function text(overrides: Partial<ProcedureText>): ProcedureText {
  return {
    ...procedureText('Titel'),
    applicability: 'Immer.',
    rationaleWhy: 'Darum.',
    positiveExamples: '- gut',
    negativeExamples: '- schlecht',
    ...overrides,
  };
}

describe('detailLayout (Handoff §5.5)', () => {
  it('shows every section of one procedure without tabs, in Handoff order', () => {
    const layout = detailLayout(criterion('1', 'A', null, { procedureTexts: [text({})] }));

    expect(layout.tabbed).toBe(false);
    expect(layout.shared).toEqual(['requirement', 'applicability', 'why', 'examples', 'ratings']);
    expect(layout.perProcedure).toEqual([]);
  });

  it('leaves out a section no procedure has text for', () => {
    const layout = detailLayout(
      criterion('1', 'A', null, {
        procedureTexts: [
          text({
            applicability: null,
            positiveExamples: null,
            negativeExamples: null,
            allowedRatings: [],
          }),
        ],
      }),
    );

    expect(layout.shared).toEqual(['requirement', 'why']);
  });

  it('puts sections with the same content in every procedure above the tabs and the others in them', () => {
    const layout = detailLayout(
      criterion('1', 'A', null, {
        procedureTexts: [
          text({ testType: 'softwareTest' }),
          text({ testType: 'appTest', requirement: 'Anders.', negativeExamples: '- anders' }),
        ],
      }),
    );

    expect(layout.tabbed).toBe(true);
    expect(layout.shared).toEqual(['applicability', 'why', 'ratings']);
    expect(layout.perProcedure).toEqual(['requirement', 'examples']);
  });

  it('shows a section that one procedure lacks in the tabs, and compares ratings as a set', () => {
    const layout = detailLayout(
      criterion('1', 'A', null, {
        procedureTexts: [
          text({ testType: 'softwareTest', allowedRatings: ['fulfilled', 'barrier'] }),
          text({
            testType: 'pdfTest',
            applicability: null,
            allowedRatings: ['barrier', 'fulfilled'],
          }),
        ],
      }),
    );

    expect(layout.shared).toEqual(['requirement', 'why', 'examples', 'ratings']);
    expect(layout.perProcedure).toEqual(['applicability']);
  });

  it('has no sections for a criterion without procedure texts', () => {
    expect(detailLayout(criterion('1', 'A', null, { procedureTexts: [] }))).toEqual({
      procedures: [],
      tabbed: false,
      shared: [],
      perProcedure: [],
    });
  });
});

describe('ratingsOf', () => {
  it('sorts the allowed ratings by the scale of BIT inklusiv', () => {
    expect(
      ratingsOf(
        text({ allowedRatings: ['notApplicable', 'blockade', 'fulfilled', 'slightRestriction'] }),
      ),
    ).toEqual(['fulfilled', 'slightRestriction', 'blockade', 'notApplicable']);
  });
});

describe('tabLabelKey (U101)', () => {
  it('gives the short label of the three BIT inklusiv tests and the full name of any other', () => {
    expect(tabLabelKey('softwareTest')).toBe('criteria.item.detail.tabs.softwareTest');
    expect(tabLabelKey('appTest')).toBe('criteria.item.detail.tabs.appTest');
    expect(tabLabelKey('pdfTest')).toBe('criteria.item.detail.tabs.pdfTest');
    expect(tabLabelKey('bikWeb')).toBe('testType.bikWeb');
  });
});
