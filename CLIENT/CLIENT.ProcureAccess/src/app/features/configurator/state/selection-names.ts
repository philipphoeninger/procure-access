// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { CriteriaSelection } from '../models/configurator-draft';
import { FilterOption, FilterOptionsLayers } from '../models/filter-options';

/** The names of a selection by layer, for the summary above the S11 list (`criteria.list.summary.*`). */
export interface SelectionNames {
  /** Null for „Keiner davon passt“: the selection sends no use case. */
  useCase: string | null;
  appTypes: string[];
  productParts: string[];
  testTypes: string[];
}

/**
 * Names the values of the selection S11 shows, in the order of the filter options. An id the
 * options do not list is shown as it is, like a catalog key without a name (`catalogNames`).
 * Always-selected product parts are part of the selection, so they are named too.
 */
export function selectionNames(selection: CriteriaSelection, options: FilterOptionsLayers | null): SelectionNames {
  const named = (values: readonly FilterOption[] | undefined, ids: readonly string[]) => {
    const listed = (values ?? []).filter((value) => ids.includes(value.id));
    const unlisted = ids.filter((id) => !listed.some((value) => value.id === id));
    return [...listed.map((value) => value.name), ...unlisted];
  };
  const useCase = selection.useCase;
  return {
    useCase:
      useCase === undefined ? null : (options?.useCases.find((value) => value.id === useCase)?.name ?? useCase),
    appTypes: named(options?.appTypes, selection.appTypeIds),
    productParts: named(options?.productParts, selection.productPartIds),
    testTypes: named(options?.testTypes, selection.testTypeIds),
  };
}
