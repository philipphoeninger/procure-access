// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { PriorityRuleText } from '@app/shared/components/priority-indicator/priority-level';

import { Criterion, MatchedRule } from '../models/criteria';
import { CatalogNames } from './catalog-names';

/** A text of the card: a UX copy key with its parameters. */
export interface CardText {
  key: string;
  params: Readonly<Record<string, string>>;
}

const PROCEDURE_KEY_PREFIX = 'PROC-';

/**
 * Anchor of a card, `k-` and the key with dots as hyphens (`k-11-2-1-1`, `k-PROC-softwareTest`),
 * so links can point at a criterion (R-LST-01, Handoff §5.5).
 */
export function cardAnchor(key: string): string {
  return `k-${key.replaceAll('.', '-')}`;
}

/** The test type of a „Geprüft nach …“ criterion, from its key `PROC-<testType>` (SPEC §8.2). */
export function procedureTestType(criterion: Criterion): string | null {
  return criterion.kind === 'testProcedure' && criterion.key.startsWith(PROCEDURE_KEY_PREFIX)
    ? criterion.key.slice(PROCEDURE_KEY_PREFIX.length)
    : null;
}

/**
 * The H2 of a requirement card: the plain-language title, or the catalog title while the release
 * has none yet (owner decision O-S4-32 (a); 2026.1 before O-1).
 */
export function requirementTitle(criterion: Criterion): string {
  return criterion.plainLanguage ?? criterion.title;
}

/**
 * `criteria.item.sourceTitle` („11.2.1.1 Ohne Maus nutzbar · BIT inklusiv“) from the first
 * procedure text; null when the criterion has none for the selected app types.
 */
export function sourceTitle(criterion: Criterion): CardText | null {
  const text = criterion.procedureTexts[0];
  return text === undefined
    ? null
    : { key: 'criteria.item.sourceTitle', params: { number: text.originalNumber, originalTitle: text.originalTitle } };
}

/**
 * The meta line of a requirement card (UX copy v18 U100, owner decision O-S4-31 (a)): product part
 * and the clause of the EN 301 549 reference — not the criterion number, which for the PDF/UA-1
 * criteria is `UA1-…`. Only the parts that exist; null when neither does.
 */
export function metaLine(criterion: Criterion, names: CatalogNames): CardText | null {
  const productPart = criterion.productPart === null ? undefined : names.productParts[criterion.productPart];
  const number = criterion.standardReferences.find((r) => r.standard === 'en301549')?.clause;
  if (productPart !== undefined && number !== undefined) {
    return { key: 'criteria.item.meta', params: { productPart, number } };
  }
  if (productPart !== undefined) {
    return { key: 'criteria.item.meta.part', params: { productPart } };
  }
  if (number !== undefined) {
    return { key: 'criteria.item.meta.standard', params: { number } };
  }
  return null;
}

/**
 * The text of the rule that gave the recommendation (CAT-02): `criteria.priority.rule.<kind>`
 * with the names of the app type and use case it names. A key the options do not list is shown
 * as it is, so the rule never goes missing.
 */
export function ruleText(rule: MatchedRule, names: CatalogNames): PriorityRuleText {
  const appType = rule.appType === null ? '' : (names.appTypes[rule.appType] ?? rule.appType);
  const useCase = rule.useCase === null ? '' : (names.useCases[rule.useCase] ?? rule.useCase);
  switch (rule.kind) {
    case 'appTypeAndUseCase':
      return { key: 'criteria.priority.rule.full', params: { appType, useCase } };
    case 'appType':
      return { key: 'criteria.priority.rule.appType', params: { appType } };
    case 'useCase':
      return { key: 'criteria.priority.rule.useCase', params: { useCase } };
    case 'default':
      return { key: 'criteria.priority.rule.default', params: {} };
  }
}
