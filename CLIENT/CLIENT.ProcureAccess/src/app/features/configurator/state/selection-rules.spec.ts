// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ConfiguratorDraft, ConfiguratorLayer, EMPTY_DRAFT } from '../models/configurator-draft';
import { ExcludingLayer, ExcludingValue, FilterOptionsLayers } from '../models/filter-options';
import { excluded, filterOptions } from '../testing/filter-options-fixture';
import {
  alwaysSelectedPartIds,
  chooseUseCase,
  exclusionReasonLines,
  exclusionReasons,
  isComplete,
  isExcluded,
  layerOfField,
  openingStep,
  selectedNames,
  stepError,
  toCatalogSelection,
  toCriteriaSelection,
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

  describe('opening step of S10 (owner decision O-S4-47 (a))', () => {
    it('opens step 1 without a request', () => {
      expect(openingStep(undefined, filled)).toBe(0);
    });

    it('opens the requested step when every earlier step is answered', () => {
      expect(openingStep('appType', filled)).toBe(1);
      expect(openingStep('appType', { ...filled, appTypeIds: [] })).toBe(1);
      expect(openingStep('testType', filled)).toBe(3);
    });

    it('never opens past the first unanswered step', () => {
      expect(openingStep('appType', EMPTY_DRAFT)).toBe(0);
      expect(openingStep('testType', { ...filled, appTypeIds: [] })).toBe(1);
    });

    it('opens step 1 for a value that is not a layer', () => {
      expect(openingStep('nonsense' as ConfiguratorLayer, filled)).toBe(0);
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

  describe('reason lines of an excluded option (R-CON-06, UX copy U98)', () => {
    const join = (names: string[]) => names.join(' | ');
    const byHr = { layer: 'useCase', id: 'hr', name: 'HR' } as const;
    const byWeb = { layer: 'appType', id: 'web', name: 'Web' } as const;
    const byClosed = { layer: 'appType', id: 'closed', name: 'Closed' } as const;
    const byAudio = { layer: 'productPart', id: 'audio', name: 'Audio' } as const;
    const byVideo = { layer: 'productPart', id: 'video', name: 'Video' } as const;
    const linesFor = (...by: ExcludingValue[]) =>
      exclusionReasonLines(excluded(options, 'testTypes', 'pdfTest', ...by).testTypes[1], join);

    it('gives an available option no line', () => {
      expect(exclusionReasonLines(options.testTypes[1], join)).toEqual([]);
    });

    it('names the use case, the app types or both in one line', () => {
      expect(linesFor(byHr)).toEqual([{ key: 'criteria.select.excluded.useCase', params: { useCase: 'HR' } }]);
      expect(linesFor(byWeb, byClosed)).toEqual([
        { key: 'criteria.select.excluded.appType', params: { appTypes: 'Web | Closed' } },
      ]);
      expect(linesFor(byHr, byWeb)).toEqual([
        { key: 'criteria.select.excluded.both', params: { useCase: 'HR', appTypes: 'Web' } },
      ]);
    });

    it('gives the product parts a line of their own after the other one', () => {
      expect(linesFor(byAudio, byVideo)).toEqual([
        { key: 'criteria.select.excluded.productPart', params: { productParts: 'Audio | Video' } },
      ]);
      expect(linesFor(byAudio, byHr, byWeb)).toEqual([
        { key: 'criteria.select.excluded.both', params: { useCase: 'HR', appTypes: 'Web' } },
        { key: 'criteria.select.excluded.productPart', params: { productParts: 'Audio' } },
      ]);
    });
  });

  describe('exclusion rules between every pair of layers (CAT-12, configurator part)', () => {
    type Pair = [ExcludingLayer, Exclude<ConfiguratorLayer, 'useCase'>];
    const PAIRS: Pair[] = [
      ['useCase', 'appType'],
      ['useCase', 'productPart'],
      ['useCase', 'testType'],
      ['appType', 'productPart'],
      ['appType', 'testType'],
      ['productPart', 'testType'],
    ];
    const SOURCES = {
      useCase: { layer: 'useCase', id: 'hr', name: 'HR' },
      appType: { layer: 'appType', id: 'web', name: 'Web' },
      productPart: { layer: 'productPart', id: 'audio', name: 'Audio' },
    } as const;
    const TARGETS = {
      appType: { list: 'appTypes', id: 'document' },
      productPart: { list: 'productParts', id: 'video' },
      testType: { list: 'testTypes', id: 'pdfTest' },
    } as const;
    const REASON_KEYS = {
      useCase: 'criteria.select.excluded.useCase',
      appType: 'criteria.select.excluded.appType',
      productPart: 'criteria.select.excluded.productPart',
    } as const;
    const toggles = {
      appType: toggleAppType,
      productPart: toggleProductPart,
      testType: toggleTestType,
    };

    it.each(PAIRS)('INV:CAT-12 a value of %s excludes a %s: the configurator refuses it and names the reason', (source, target) => {
      const { list, id } = TARGETS[target];
      const layers: FilterOptionsLayers = excluded(options, list, id, SOURCES[source]);
      const option = layers[list].find((o) => o.id === id)!;

      expect(toggles[target](filled, id, layers).draft).toBe(filled);
      expect(exclusionReasonLines(option, (names) => names.join()).map((line) => line.key)).toEqual([
        REASON_KEYS[source],
      ]);
    });
  });

  describe('reset of the lower layers (R-CON-07)', () => {
    it('INV:CFG-01 clears steps 2–4 except Basisfunktionalität when the use case changes', () => {
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

    it('adds the test types for the criteria endpoint (SPEC §8.2)', () => {
      expect(toCriteriaSelection(filled)).toEqual({
        useCase: 'hr',
        appTypeIds: ['web', 'closed'],
        productPartIds: ['base', 'audio'],
        testTypeIds: ['softwareTest'],
      });
      expect(toCriteriaSelection({ ...filled, useCase: { kind: 'none' } }).useCase).toBeUndefined();
    });
  });

  describe('„Bisherige Auswahl“ (R-CON-08)', () => {
    it('names the selected values in the order of the options, Basisfunktionalität included', () => {
      const draft: ConfiguratorDraft = { ...filled, appTypeIds: ['closed', 'web'] };

      expect(selectedNames(draft, options, 'appType')).toEqual(['Web', 'Closed']);
      expect(selectedNames(draft, options, 'productPart')).toEqual(['Base', 'Audio']);
      expect(selectedNames(draft, options, 'testType')).toEqual(['Software']);
    });

    it('leaves out an id the options no longer list', () => {
      expect(selectedNames({ ...filled, appTypeIds: ['web', 'gone'] }, options, 'appType')).toEqual(['Web']);
    });
  });

  describe('field paths of the server check (R-CON-10)', () => {
    it.each([
      ['useCase', 'useCase'],
      ['appTypeIds[1]', 'appType'],
      ['appTypeIds', 'appType'],
      ['productPartIds[0]', 'productPart'],
      ['testTypeIds[2]', 'testType'],
      ['release', null],
      ['$', null],
      ['toString', null],
    ])('maps %s to %s', (path, layer) => {
      expect(layerOfField(path)).toBe(layer);
    });
  });
});
