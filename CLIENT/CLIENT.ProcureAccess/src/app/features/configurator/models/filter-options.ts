// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * Answer of `GET /api/v1/catalog/filter-options` (SPEC §8.1).
 * Names and hints are catalog texts in the language of the answer; the client never shows a reason
 * text from the server, it builds the reason from `criteria.select.excluded.*` and `excludedBy[]`.
 */
export interface FilterOptionsResponse {
  release: string;
  layers: FilterOptionsLayers;
}

export interface FilterOptionsLayers {
  useCases: UseCaseOption[];
  appTypes: AppTypeOption[];
  productParts: ProductPartOption[];
  testTypes: TestTypeOption[];
}

/** A higher layer that can exclude values of a lower one (`excludedBy[].layer`). */
export type ExcludingLayer = 'useCase' | 'appType' | 'productPart';

/** A selected higher-layer value that excludes an option. */
export interface ExcludingValue {
  layer: ExcludingLayer;
  id: string;
  name: string;
}

/** What every option of every layer has; an empty `excludedBy` means available. */
export interface FilterOption {
  id: string;
  name: string;
  excludedBy: ExcludingValue[];
}

export interface UseCaseOption extends FilterOption {
  hint: string | null;
}

export interface AppTypeOption extends FilterOption {
  hint: string | null;
}

export interface ProductPartOption extends FilterOption {
  explanation: string;
  /** Basisfunktionalität: always part of the selection and never removable (R-CON-04). */
  alwaysSelected: boolean;
}

export interface TestTypeOption extends FilterOption {
  url: string;
}
