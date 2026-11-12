// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DEFAULT_DECISION } from '../models/criteria-list';
import { criteriaAnswer } from '../testing/criteria-fixture';
import { fileNameOf, toExportRequest } from './export-request';

describe('toExportRequest (POST /api/v1/exports, S4-19 C123)', () => {
  const answer = criteriaAnswer();
  const selection = { useCase: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: ['softwareTest'] };

  it('names the release of the answer, the selection by key and every criterion once by revision', () => {
    const request = toExportRequest('xlsx', answer, selection, {});

    expect(request.format).toBe('xlsx');
    expect(request.selection).toEqual({
      releaseVersion: 'T.1',
      useCaseTemplateId: 'hr',
      appTypeIds: ['web'],
      productPartIds: ['base'],
      testTypeIds: ['softwareTest'],
    });
    expect(request.criteria).toEqual(
      answer.criteria.map((criterion) => ({ criterionRevisionId: criterion.revisionId, isIncluded: true })),
    );
  });

  it('sends no use case for „Keiner davon passt“', () => {
    const request = toExportRequest('csv', answer, { ...selection, useCase: undefined }, {});

    expect('useCaseTemplateId' in request.selection).toBe(false);
  });

  it('sends an override, a weight only while B applies and a note only when it has text — never the recommendation', () => {
    const [first, second, third] = answer.criteria;
    const request = toExportRequest('xlsx', answer, selection, {
      [first.key]: { ...DEFAULT_DECISION, included: false, note: 'Bitte prüfen' },
      [second.key]: { ...DEFAULT_DECISION, overrideLevel: 'B', overrideWeight: 7 },
      [third.key]: { ...DEFAULT_DECISION, overrideLevel: 'A', overrideWeight: 7 },
    });

    expect(request.criteria[0]).toEqual({ criterionRevisionId: first.revisionId, isIncluded: false, note: 'Bitte prüfen' });
    expect(request.criteria[1]).toEqual({
      criterionRevisionId: second.revisionId,
      isIncluded: true,
      overrideLevel: 'B',
      overrideWeight: 7,
    });
    expect(request.criteria[2]).toEqual({ criterionRevisionId: third.revisionId, isIncluded: true, overrideLevel: 'A' });
    expect(JSON.stringify(request)).not.toContain('resolvedPriority');
  });
});

describe('fileNameOf', () => {
  it.each([
    [`attachment; filename=ProcureAccess_Kriterien_T.1_2026-11-12.xlsx; filename*=UTF-8''ProcureAccess_Kriterien_T.1_2026-11-12.xlsx`, 'ProcureAccess_Kriterien_T.1_2026-11-12.xlsx'],
    [`attachment; filename*=UTF-8''Liste%20%C3%BCber.csv`, 'Liste über.csv'],
    ['attachment; filename="liste.csv"', 'liste.csv'],
    ['attachment', null],
    [null, null],
  ])('reads %s as %s', (header, expected) => {
    expect(fileNameOf(header)).toBe(expected);
  });
});
