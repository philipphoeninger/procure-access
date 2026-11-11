// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { filterOptionsAnswer } from '../testing/filter-options-fixture';
import { selectionNames } from './selection-names';

describe('selectionNames (S11 summary)', () => {
  const options = filterOptionsAnswer().layers;
  const [useCase] = options.useCases;
  const [web, desktop] = options.appTypes;
  const [base] = options.productParts;

  it('names every layer in the order of the options', () => {
    if (!useCase || !web || !desktop || !base) throw new Error('fixture has options');

    const names = selectionNames(
      { useCase: useCase.id, appTypeIds: [desktop.id, web.id], productPartIds: [base.id], testTypeIds: [] },
      options,
    );

    expect(names).toEqual({ useCase: useCase.name, appTypes: [web.name, desktop.name], productParts: [base.name], testTypes: [] });
  });

  it('gives null for „Keiner davon passt“ and shows an id the options do not list as it is', () => {
    const names = selectionNames({ appTypeIds: ['gone'], productPartIds: [], testTypeIds: [] }, options);

    expect(names.useCase).toBeNull();
    expect(names.appTypes).toEqual(['gone']);
  });

  it('shows the ids when no options are loaded', () => {
    expect(selectionNames({ useCase: 'hr', appTypeIds: ['web'], productPartIds: [], testTypeIds: [] }, null)).toEqual({
      useCase: 'hr',
      appTypes: ['web'],
      productParts: [],
      testTypes: [],
    });
  });
});
