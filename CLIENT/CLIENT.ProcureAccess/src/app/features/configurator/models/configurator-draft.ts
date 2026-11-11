// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/** The four layers of the configurator in their fixed order (R-CON-01). */
export type ConfiguratorLayer = 'useCase' | 'appType' | 'productPart' | 'testType';

export const CONFIGURATOR_LAYERS: readonly ConfiguratorLayer[] = [
  'useCase',
  'appType',
  'productPart',
  'testType',
];

/**
 * Answer of step 1: not answered yet, „Keiner davon passt“ (no use case is sent, the general
 * recommendations apply) or exactly one use case (R-CON-02).
 */
export type UseCaseChoice =
  | { kind: 'unanswered' }
  | { kind: 'none' }
  | { kind: 'useCase'; id: string };

/**
 * The selection of the configurator (R-CON-09): lives only in memory, never in browser storage.
 * `productPartIds` holds the always-selected parts too, because the server counts only named
 * parts as selected (SPEC §8.1).
 */
export interface ConfiguratorDraft {
  useCase: UseCaseChoice;
  appTypeIds: string[];
  productPartIds: string[];
  testTypeIds: string[];
}

export const EMPTY_DRAFT: ConfiguratorDraft = {
  useCase: { kind: 'unanswered' },
  appTypeIds: [],
  productPartIds: [],
  testTypeIds: [],
};

/**
 * What a change of a higher layer cleared (R-CON-07, CFG-01). `changedLayer` picks the notice
 * text (`criteria.select.reset.text.<changedLayer>`); `clearedLayers` names the lower layers that
 * held a selection — always-selected product parts do not count.
 */
export interface ResetReport {
  changedLayer: Exclude<ConfiguratorLayer, 'testType'>;
  clearedLayers: Exclude<ConfiguratorLayer, 'useCase'>[];
}

/** Why „Weiter“ cannot leave a step (`criteria.select.error.<code>`). */
export type StepError = 'noUseCase' | 'noAppType';

/** The selection as the catalog endpoints read it (SPEC §8.1, §8.2). */
export interface CatalogSelection {
  useCase?: string;
  appTypeIds: string[];
  productPartIds: string[];
}

/** The selection as `GET /api/v1/catalog/criteria` reads it: the filter selection plus the test types (SPEC §8.2). */
export interface CriteriaSelection extends CatalogSelection {
  testTypeIds: string[];
}
