// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  CatalogSelection,
  CONFIGURATOR_LAYERS,
  ConfiguratorDraft,
  CriteriaSelection,
  ConfiguratorLayer,
  ResetReport,
  StepError,
  UseCaseChoice,
} from '../models/configurator-draft';
import {
  ExcludingLayer,
  FilterOption,
  FilterOptionsLayers,
} from '../models/filter-options';

/**
 * Selection rules of the configurator S10 (R-CON-02 – R-CON-07, Handoff §5.4), as pure functions.
 *
 * Every change returns a new draft, or the same draft object when nothing changes: an unknown or
 * excluded option, an always-selected product part, or the use case that is already chosen.
 * Changing a layer clears every lower layer except the always-selected product parts (CFG-01),
 * also when the lower values would still be allowed — the reset never keeps a selection silently.
 */
export interface SelectionChange {
  draft: ConfiguratorDraft;
  /** Set only when the change cleared a lower layer that held a selection. */
  reset: ResetReport | null;
}

/** An option is available when no selected higher-layer value excludes it (SPEC §8.1). */
export function isExcluded(option: FilterOption): boolean {
  return option.excludedBy.length > 0;
}

/** Names of the selected values that exclude an option, by layer, for the reason line (R-CON-06). */
export function exclusionReasons(option: FilterOption): Record<ExcludingLayer, string[]> {
  const reasons: Record<ExcludingLayer, string[]> = { useCase: [], appType: [], productPart: [] };
  for (const value of option.excludedBy) {
    reasons[value.layer].push(value.name);
  }
  return reasons;
}

/** One reason line under an excluded option: a `criteria.select.excluded.*` key and its values. */
export interface ReasonLine {
  key:
    | 'criteria.select.excluded.useCase'
    | 'criteria.select.excluded.appType'
    | 'criteria.select.excluded.both'
    | 'criteria.select.excluded.productPart';
  params: Record<string, string>;
}

/**
 * The reason lines of an excluded option (R-CON-06, Handoff §5.4, UX copy U98): use case and app
 * types share one line (`both` when both exclude it), product parts get a line of their own after
 * it, so the reason names every value that excludes the option. `joinNames` joins several names of
 * one layer („A oder B“); an available option has no line.
 */
export function exclusionReasonLines(
  option: FilterOption,
  joinNames: (names: string[]) => string,
): ReasonLine[] {
  const { useCase, appType, productPart } = exclusionReasons(option);
  const lines: ReasonLine[] = [];
  if (useCase.length > 0 && appType.length > 0) {
    lines.push({
      key: 'criteria.select.excluded.both',
      params: { useCase: joinNames(useCase), appTypes: joinNames(appType) },
    });
  } else if (useCase.length > 0) {
    lines.push({ key: 'criteria.select.excluded.useCase', params: { useCase: joinNames(useCase) } });
  } else if (appType.length > 0) {
    lines.push({ key: 'criteria.select.excluded.appType', params: { appTypes: joinNames(appType) } });
  }
  if (productPart.length > 0) {
    lines.push({
      key: 'criteria.select.excluded.productPart',
      params: { productParts: joinNames(productPart) },
    });
  }
  return lines;
}

/** Ids of the product parts that are always selected (Basisfunktionalität, R-CON-04). */
export function alwaysSelectedPartIds(options: FilterOptionsLayers): string[] {
  return options.productParts.filter((part) => part.alwaysSelected).map((part) => part.id);
}

/**
 * Adds the always-selected product parts to the draft, e.g. after the first filter options
 * arrived. Returns the same draft when they are already in it.
 */
export function withAlwaysSelectedParts(
  draft: ConfiguratorDraft,
  options: FilterOptionsLayers,
): ConfiguratorDraft {
  const missing = alwaysSelectedPartIds(options).filter((id) => !draft.productPartIds.includes(id));
  if (missing.length === 0) {
    return draft;
  }
  return {
    ...draft,
    productPartIds: inOptionOrder(options.productParts, [...draft.productPartIds, ...missing]),
  };
}

/** Step 1: one use case or „Keiner davon passt“ (R-CON-02). */
export function chooseUseCase(
  draft: ConfiguratorDraft,
  choice: Exclude<UseCaseChoice, { kind: 'unanswered' }>,
  options: FilterOptionsLayers,
): SelectionChange {
  if (sameUseCase(draft.useCase, choice)) {
    return unchanged(draft);
  }
  if (choice.kind === 'useCase' && !isSelectable(options.useCases, choice.id)) {
    return unchanged(draft);
  }
  return clearBelow({ ...draft, useCase: choice }, draft, 'useCase', options);
}

/** Step 2: any number of app types (R-CON-03); at least one is checked by {@link stepError}. */
export function toggleAppType(
  draft: ConfiguratorDraft,
  id: string,
  options: FilterOptionsLayers,
): SelectionChange {
  if (!isSelectable(options.appTypes, id)) {
    return unchanged(draft);
  }
  const appTypeIds = toggled(options.appTypes, draft.appTypeIds, id);
  return clearBelow({ ...draft, appTypeIds }, draft, 'appType', options);
}

/** Step 3: any number of product parts; an always-selected part cannot be removed (R-CON-04). */
export function toggleProductPart(
  draft: ConfiguratorDraft,
  id: string,
  options: FilterOptionsLayers,
): SelectionChange {
  const part = options.productParts.find((p) => p.id === id);
  if (part === undefined || part.alwaysSelected || isExcluded(part)) {
    return unchanged(draft);
  }
  const productPartIds = toggled(options.productParts, draft.productPartIds, id);
  return clearBelow({ ...draft, productPartIds }, draft, 'productPart', options);
}

/** Step 4: optional test types (R-CON-05); the lowest layer, so nothing is cleared. */
export function toggleTestType(
  draft: ConfiguratorDraft,
  id: string,
  options: FilterOptionsLayers,
): SelectionChange {
  if (!isSelectable(options.testTypes, id)) {
    return unchanged(draft);
  }
  return unchanged({ ...draft, testTypeIds: toggled(options.testTypes, draft.testTypeIds, id) });
}

/**
 * Why „Weiter“ cannot leave the step of a layer (R-CON-01), or null. Step 3 always holds the
 * always-selected parts and step 4 may stay empty (CFG-04).
 */
export function stepError(layer: ConfiguratorLayer, draft: ConfiguratorDraft): StepError | null {
  if (layer === 'useCase' && draft.useCase.kind === 'unanswered') {
    return 'noUseCase';
  }
  if (layer === 'appType' && draft.appTypeIds.length === 0) {
    return 'noAppType';
  }
  return null;
}

/**
 * The step S10 opens at (owner decision O-S4-47 (a)): the requested layer, but never past the first
 * step the draft has not answered yet — the linear stepper would not let the user get there either.
 * Without a request S10 opens at step 1.
 */
export function openingStep(requested: ConfiguratorLayer | undefined, draft: ConfiguratorDraft): number {
  const index = requested === undefined ? 0 : Math.max(0, CONFIGURATOR_LAYERS.indexOf(requested));
  const firstOpen = CONFIGURATOR_LAYERS.findIndex((layer) => stepError(layer, draft) !== null);
  return firstOpen === -1 ? index : Math.min(index, firstOpen);
}

/** True when „Kriterienliste anzeigen“ may send the selection (the server checks it again, R-CON-10). */
export function isComplete(draft: ConfiguratorDraft): boolean {
  return stepError('useCase', draft) === null && stepError('appType', draft) === null;
}

/** The selection as the filter-options endpoint reads it; „Keiner davon passt“ sends no use case. */
export function toCatalogSelection(draft: ConfiguratorDraft): CatalogSelection {
  return {
    ...(draft.useCase.kind === 'useCase' ? { useCase: draft.useCase.id } : {}),
    appTypeIds: draft.appTypeIds,
    productPartIds: draft.productPartIds,
  };
}

/** The selection as the criteria endpoint reads it: the filter selection plus the test types (SPEC §8.2). */
export function toCriteriaSelection(draft: ConfiguratorDraft): CriteriaSelection {
  return { ...toCatalogSelection(draft), testTypeIds: draft.testTypeIds };
}

/**
 * Names of the selected values of a multi-choice layer in the order of the options, for
 * „Bisherige Auswahl“ (R-CON-08). Always-selected product parts are part of the selection and
 * named too; an id the options no longer list is left out.
 */
export function selectedNames(
  draft: ConfiguratorDraft,
  options: FilterOptionsLayers,
  layer: Exclude<ConfiguratorLayer, 'useCase'>,
): string[] {
  const [values, ids] = {
    appType: [options.appTypes, draft.appTypeIds],
    productPart: [options.productParts, draft.productPartIds],
    testType: [options.testTypes, draft.testTypeIds],
  }[layer] as [FilterOption[], string[]];
  return values.filter((o) => ids.includes(o.id)).map((o) => o.name);
}

/**
 * The layer of a field path in the Problem Details of the criteria endpoint (SPEC §8.2: keys as
 * sent, e.g. `appTypeIds[1]`), so S10 can show the error at its step; null for any other path.
 */
export function layerOfField(path: string): ConfiguratorLayer | null {
  const name = path.replace(/\[\d*\]$/, '');
  const layers: Readonly<Record<string, ConfiguratorLayer>> = {
    useCase: 'useCase',
    appTypeIds: 'appType',
    productPartIds: 'productPart',
    testTypeIds: 'testType',
  };
  return Object.hasOwn(layers, name) ? layers[name] : null;
}

function unchanged(draft: ConfiguratorDraft): SelectionChange {
  return { draft, reset: null };
}

function sameUseCase(a: UseCaseChoice, b: UseCaseChoice): boolean {
  if (a.kind === 'useCase' && b.kind === 'useCase') {
    return a.id === b.id;
  }
  return a.kind === b.kind;
}

function isSelectable(options: FilterOption[], id: string): boolean {
  const option = options.find((o) => o.id === id);
  return option !== undefined && !isExcluded(option);
}

/** Adds or removes an id; the result keeps the order of the options, so requests stay stable. */
function toggled(options: FilterOption[], ids: string[], id: string): string[] {
  return ids.includes(id)
    ? ids.filter((i) => i !== id)
    : inOptionOrder(options, [...ids, id]);
}

function inOptionOrder(options: FilterOption[], ids: string[]): string[] {
  return options.filter((o) => ids.includes(o.id)).map((o) => o.id);
}

/** Clears the layers below `changed` in `next` and reports those that held a selection in `before`. */
function clearBelow(
  next: ConfiguratorDraft,
  before: ConfiguratorDraft,
  changed: ResetReport['changedLayer'],
  options: FilterOptionsLayers,
): SelectionChange {
  const always = alwaysSelectedPartIds(options);
  const cleared: ResetReport['clearedLayers'] = [];
  const draft = { ...next };

  if (changed === 'useCase') {
    if (before.appTypeIds.length > 0) {
      cleared.push('appType');
    }
    draft.appTypeIds = [];
  }
  if (changed === 'useCase' || changed === 'appType') {
    if (before.productPartIds.some((id) => !always.includes(id))) {
      cleared.push('productPart');
    }
    draft.productPartIds = always;
  }
  if (before.testTypeIds.length > 0) {
    cleared.push('testType');
  }
  draft.testTypeIds = [];

  return { draft, reset: cleared.length > 0 ? { changedLayer: changed, clearedLayers: cleared } : null };
}
