// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Criterion, CriteriaResponse, PriorityLevel, ProcedureText } from '../models/criteria';

/**
 * A small answer of `GET /api/v1/catalog/criteria` for the S11 tests, shaped as the Catalog module
 * sends it (SPEC §8.2). Texts are placeholders: the tests check rules, not catalog texts.
 *
 * - `11.1.1` A, with a plain-language title and a procedure text „Nicht-Text-Inhalte prüfen“
 * - `11.2.2` B with weight 3
 * - `11.3.3` Info, without plain-language title (as 2026.1 before O-1)
 * - `PROC-softwareTest` B without weight, a „Geprüft nach …“ criterion
 */
export function criteriaAnswer(release = 'T.1'): CriteriaResponse {
  return {
    release,
    criteria: [
      criterion('11.1.1', 'A', null, {
        title: 'Nicht-Text-Inhalt',
        plainLanguage: 'Bilder haben einen Alternativtext',
        procedureTexts: [procedureText('Nicht-Text-Inhalte prüfen')],
      }),
      criterion('11.2.2', 'B', 3, {
        title: 'Tastaturbedienung',
        plainLanguage: 'Alles geht mit der Tastatur',
      }),
      criterion('11.3.3', 'Info', null, { title: 'Überschriften', plainLanguage: null }),
      {
        ...criterion('PROC-softwareTest', 'B', null, {
          title: 'Geprüft nach Softwaretest',
          plainLanguage: null,
        }),
        kind: 'testProcedure',
        number: null,
        productPart: null,
        appliesTo: [],
        allowedRatings: [],
      },
    ],
    appTypesWithoutCriteria: [],
  };
}

/** A requirement criterion with the given recommendation; `overrides` replace single fields. */
export function criterion(
  key: string,
  level: PriorityLevel,
  weight: number | null,
  overrides: Partial<Criterion> = {},
): Criterion {
  return {
    revisionId: 100 + key.length,
    key,
    kind: 'requirement',
    number: key,
    title: `Title ${key}`,
    plainLanguage: `Plain ${key}`,
    productPart: 'base',
    appliesTo: [{ id: 'web', name: 'Website oder Webanwendung' }],
    standardReferences: [{ standard: 'en301549', clause: key, url: null }],
    allowedRatings: ['fulfilled', 'restriction', 'barrier', 'blockade'],
    procedureTexts: [],
    resolvedPriority: {
      level,
      weight,
      rationale: `Rationale ${key}`,
      source: 'expertJudgement',
      matchedRule: { kind: 'default', appType: null, useCase: null },
    },
    ...overrides,
  };
}

/** A BIT inklusiv text of the Software-Test with the given original title. */
export function procedureText(originalTitle: string): ProcedureText {
  return {
    testType: 'softwareTest',
    originalNumber: '1.1',
    originalTitle,
    applicability: null,
    requirement: 'Requirement text',
    rationaleWhy: null,
    allowedRatings: ['fulfilled', 'restriction', 'barrier', 'blockade'],
    positiveExamples: null,
    negativeExamples: null,
    sourceUrl: 'https://example.invalid/bit#1.1',
  };
}
