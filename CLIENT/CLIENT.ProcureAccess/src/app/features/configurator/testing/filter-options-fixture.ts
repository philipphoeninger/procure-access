// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  ExcludingValue,
  FilterOption,
  FilterOptionsLayers,
  FilterOptionsResponse,
} from '../models/filter-options';

/**
 * A small set of filter options for the configurator tests, shaped like an answer of
 * `GET /api/v1/catalog/filter-options` (SPEC §8.1). Names are placeholders: the tests check rules,
 * not catalog texts. `excludedBy` is empty everywhere; tests add what they need with {@link excluded}.
 */
export function filterOptions(): FilterOptionsLayers {
  return {
    useCases: [
      { id: 'hr', name: 'HR', hint: null, excludedBy: [] },
      { id: 'citizen', name: 'Citizen', hint: null, excludedBy: [] },
    ],
    appTypes: [
      { id: 'web', name: 'Web', hint: null, excludedBy: [] },
      { id: 'document', name: 'Document', hint: null, excludedBy: [] },
      { id: 'closed', name: 'Closed', hint: 'closed hint', excludedBy: [] },
    ],
    productParts: [
      { id: 'base', name: 'Base', explanation: '', alwaysSelected: true, excludedBy: [] },
      { id: 'audio', name: 'Audio', explanation: '', alwaysSelected: false, excludedBy: [] },
      { id: 'video', name: 'Video', explanation: '', alwaysSelected: false, excludedBy: [] },
    ],
    testTypes: [
      { id: 'softwareTest', name: 'Software', url: 'https://example.invalid/sw', excludedBy: [] },
      { id: 'pdfTest', name: 'PDF', url: 'https://example.invalid/pdf', excludedBy: [] },
    ],
  };
}

export function filterOptionsAnswer(
  layers: FilterOptionsLayers = filterOptions(),
  release = 'T.1',
): FilterOptionsResponse {
  return { release, layers };
}

/** Marks an option of a layer as excluded by the given values. */
export function excluded(
  layers: FilterOptionsLayers,
  layer: keyof FilterOptionsLayers,
  id: string,
  ...by: ExcludingValue[]
): FilterOptionsLayers {
  const options: FilterOption[] = layers[layer];
  return {
    ...layers,
    [layer]: options.map((o) => (o.id === id ? { ...o, excludedBy: by } : o)),
  };
}
