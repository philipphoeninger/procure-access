// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ConfiguratorDraft, EMPTY_DRAFT } from '../models/configurator-draft';
import { excluded, filterOptions } from '../testing/filter-options-fixture';
import {
  alwaysSelectedPartIds,
  chooseUseCase,
  exclusionReasons,
  isComplete,
  isExcluded,
  stepError,
  toCatalogSelection,
  toggleAppType,
  toggleProductPart,
  toggleTestType,
  withAlwaysSelectedParts,
} from './selection-rules';

const options = filterOptions();

/** A draft with every layer filled: hr, web + closed, base + audio, softwareTest. */
const filled: ConfiguratorDraft = {
  useCase: { kind: 'useCase', id: 'hr' },
  appTypeIds: ['web', 'closed'],
  productPartIds: ['base', 'audio'],
  testTypeIds: ['softwareTest'],
};

describe('selection rules', () => {
  describe('use case (R-CON-02)', () => {
    it('chooses one use case', () => {
      const { draft, reset } = chooseUseCase(EMPTY_DRAFT, { kind: 'useCase', id: 'hr' }, options);

      expect(draft.useCase).toEqual({ kind: 'useCase', id: 'hr' });
      expect(reset).toBeNull();
    });

    it('replaces the use case instead of adding a second one', () => {
      const first = chooseUseCase(EMPTY_DRAFT, { kind: 'useCase', id: 'hr' }, options).draft;

      const { draft } = chooseUseCase(first, { kind: 'useCase', id: 'citizen' }, options);

      expect(draft.useCase).toEqual({ kind: 'useCase', id: 'citizen' });
    });

    it('takes „Keiner davon passt“ as an answer that sends no use case', () => {
      const { draft } = chooseUseCase(filled, { kind: 'none' }, options);

      expect(draft.useCase).toEqual({ kind: 'none' });
      expect(stepError('useCase', draft)).toBeNull();
      expect(toCatalogSelection(draft)).not.toHaveProperty('useCase');
    });

    it('ignores an unknown use case', () => {
      const result = chooseUseCase(filled, { kind: 'useCase', id: 'unknown' }, options);

      expect(result.draft).toBe(filled);
      expect(result.reset).toBeNull();
    });

    it('keeps the draft when the same use case is chosen again', () => {
      const result = chooseUseCase(filled, { kind: 'useCase', id: 'hr' }, options);

      expect(result.draft).toBe(filled);
      expect(result.reset).toBeNull();
    });
  });

  describe('step errors (R-CON-01, R-CON-03)', () => {
    it('asks for a use case while step 1 is unanswered', () => {
      expect(stepError('useCase', EMPTY_DRAFT)).toBe('noUseCase');
    });

    it('asks for at least one app type', () => {
      expect(stepError('appType', { ...filled, appTypeIds: [] })).toBe('noAppType');
      expect(stepError('appType', filled)).toBeNull();
    });

    it('lets step 3 and the optional step 4 go on without a choice (CFG-04 rule)', () => {
      expect(stepError('productPart', EMPTY_DRAFT)).toBeNull();
      expect(stepError('testType', EMPTY_DRAFT)).toBeNull();
    });

    it('calls a selection complete with an answered use case and at least one app type', () => {
      expect(isComplete(filled)).toBe(true);
      expect(isComplete({ ...filled, testTypeIds: [] })).toBe(true);
      expect(isComplete({ ...filled, useCase: { kind: 'none' } })).toBe(true);
      expect(isComplete({ ...filled, appTypeIds: [] })).toBe(false);
      expect(isComplete({ ...filled, useCase: { kind: 'unanswered' } })).toBe(false);
    });
  });

  describe('Basisfunktionalität (R-CON-04)', () => {
    it('adds the always-selected parts to a draft', () => {
      const draft = withAlwaysSelectedParts(EMPTY_DRAFT, options);

      expect(alwaysSelectedPartIds(options)).toEqual(['base']);
      expect(draft.productPartIds).toEqual(['base']);
    });

    it('returns the same draft when the always-selected parts are already in it', () => {
      expect(withAlwaysSelectedParts(filled, options)).toBe(filled);
    });

    it('cannot remove an always-selected part', () => {
      const result = toggleProductPart(filled, 'base', options);

      expect(result.draft).toBe(filled);
    });

    it('keeps the always-selected parts when a higher layer resets step 3', () => {
      const { draft } = chooseUseCase(filled, { kind: 'useCase', id: 'citizen' }, options);

      expect(draft.productPartIds).toEqual(['base']);
    });
  });

  describe('multi-select layers (R-CON-03 – R-CON-05)', () => {
    it('adds and removes app types in the order of the options', () => {
      const added = toggleAppType({ ...filled, appTypeIds: ['closed'] }, 'web', options).draft;
      expect(added.appTypeIds).toEqual(['web', 'closed']);

      const removed = toggleAppType(added, 'closed', options).draft;
      expect(removed.appTypeIds).toEqual(['web']);
    });

    it('adds and removes product parts and test types', () => {
      const parts = toggleProductPart(filled, 'video', options).draft;
      expect(parts.productPartIds).toEqual(['base', 'audio', 'video']);

      const tests = toggleTestType(filled, 'pdfTest', options).draft;
      expect(tests.testTypeIds).toEqual(['softwareTest', 'pdfTest']);
      expect(toggleTestType(tests, 'softwareTest', options).draft.testTypeIds).toEqual(['pdfTest']);
    });

    it('ignores unknown ids in every layer', () => {
      expect(toggleAppType(filled, 'unknown', options).draft).toBe(filled);
      expect(toggleProductPart(filled, 'unknown', options).draft).toBe(filled);
      expect(toggleTestType(filled, 'unknown', options).draft).toBe(filled);
    });
  });

  describe('exclusions from excludedBy[] (R-CON-06)', () => {
    const byHr = { layer: 'useCase', id: 'hr', name: 'HR' } as const;
    const byWeb = { layer: 'appType', id: 'web', name: 'Web' } as const;
    const byClosed = { layer: 'appType', id: 'closed', name: 'Closed' } as const;

    it('treats an option with an excludedBy entry as excluded', () => {
      const layers = excluded(options, 'appTypes', 'document', byHr);

      expect(isExcluded(layers.appTypes[1])).toBe(true);
      expect(isExcluded(layers.appTypes[0])).toBe(false);
    });

    it('changes nothing when an excluded option is chosen, in every layer', () => {
      let layers = excluded(options, 'useCases', 'citizen', byHr);
      layers = excluded(layers, 'appTypes', 'document', byHr);
      layers = excluded(layers, 'productParts', 'video', byWeb);
      layers = excluded(layers, 'testTypes', 'pdfTest', byWeb);

      expect(chooseUseCase(filled, { kind: 'useCase', id: 'citizen' }, layers).draft).toBe(filled);
      expect(toggleAppType(filled, 'document', layers).draft).toBe(filled);
      expect(toggleProductPart(filled, 'video', layers).draft).toBe(filled);
      expect(toggleTestType(filled, 'pdfTest', layers).draft).toBe(filled);
    });

    it('groups the names of the excluding values by layer for the reason line', () => {
      const layers = excluded(options, 'testTypes', 'pdfTest', byHr, byWeb, byClosed);

      expect(exclusionReasons(layers.testTypes[1])).toEqual({
        useCase: ['HR'],
        appType: ['Web', 'Closed'],
        productPart: [],
      });
    });
  });

  describe('reset of the lower layers (R-CON-07)', () => {
    it('clears steps 2–4 except Basisfunktionalität when the use case changes', () => {
      const { draft, reset } = chooseUseCase(filled, { kind: 'none' }, options);

      expect(draft).toEqual({
        useCase: { kind: 'none' },
        appTypeIds: [],
        productPartIds: ['base'],
        testTypeIds: [],
      });
      expect(reset).toEqual({
        changedLayer: 'useCase',
        clearedLayers: ['appType', 'productPart', 'testType'],
      });
    });

    it('clears steps 3–4 when an app type is added, also when the lower values would still fit', () => {
      const { draft, reset } = toggleAppType(filled, 'document', options);

      expect(draft.appTypeIds).toEqual(['web', 'document', 'closed']);
      expect(draft.productPartIds).toEqual(['base']);
      expect(draft.testTypeIds).toEqual([]);
      expect(reset).toEqual({ changedLayer: 'appType', clearedLayers: ['productPart', 'testType'] });
    });

    it('clears step 4 when a product part changes', () => {
      const { draft, reset } = toggleProductPart(filled, 'audio', options);

      expect(draft.productPartIds).toEqual(['base']);
      expect(draft.testTypeIds).toEqual([]);
      expect(reset).toEqual({ changedLayer: 'productPart', clearedLayers: ['testType'] });
    });

    it('reports only the lower layers that held a selection', () => {
      const partial: ConfiguratorDraft = { ...filled, productPartIds: ['base'], testTypeIds: [] };

      const { reset } = chooseUseCase(partial, { kind: 'useCase', id: 'citizen' }, options);

      expect(reset).toEqual({ changedLayer: 'useCase', clearedLayers: ['appType'] });
    });

    it('reports no reset when the lower layers were empty', () => {
      const firstAnswer = withAlwaysSelectedParts(EMPTY_DRAFT, options);

      expect(chooseUseCase(firstAnswer, { kind: 'useCase', id: 'hr' }, options).reset).toBeNull();
      expect(toggleAppType({ ...firstAnswer, useCase: { kind: 'none' } }, 'web', options).reset).toBeNull();
    });

    it('never resets anything for a test type, the lowest layer', () => {
      expect(toggleTestType(filled, 'pdfTest', options).reset).toBeNull();
    });
  });

  describe('selection sent to the catalog', () => {
    it('sends the use case, app types and product parts, not the test types', () => {
      expect(toCatalogSelection(filled)).toEqual({
        useCase: 'hr',
        appTypeIds: ['web', 'closed'],
        productPartIds: ['base', 'audio'],
      });
    });
  });
});
