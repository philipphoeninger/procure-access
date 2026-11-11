// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Criterion, ProcedureText, RatingName } from '../models/criteria';

/**
 * The catalog sections of the detail panel, in the order of Handoff §5.5, with their H3 key.
 * „Warum diese Priorität?“ comes before them (S4-15a); „So wird es geprüft“, „Gilt für“ and the
 * source line after them.
 */
export type DetailSectionKind = 'requirement' | 'applicability' | 'why' | 'examples' | 'ratings';

export const DETAIL_SECTIONS: readonly DetailSectionKind[] = [
  'requirement',
  'applicability',
  'why',
  'examples',
  'ratings',
];

export const DETAIL_SECTION_TITLE_KEYS: Readonly<Record<DetailSectionKind, string>> = {
  requirement: 'criteria.item.detail.requirement',
  applicability: 'criteria.item.detail.applicability',
  why: 'criteria.item.detail.why',
  examples: 'criteria.item.detail.examples',
  ratings: 'criteria.item.detail.ratings',
};

/** The rating scale of BIT inklusiv in its fixed order (SPEC §8.2), each with its UX key. */
export const RATING_KEYS: Readonly<Record<RatingName, string>> = {
  fulfilled: 'assessment.value.fulfilled',
  slightRestriction: 'assessment.value.slightRestriction',
  restriction: 'assessment.value.restriction',
  barrier: 'assessment.value.barrier',
  blockade: 'assessment.value.blockade',
  notApplicable: 'assessment.value.notApplicable',
};

const RATING_ORDER = Object.keys(RATING_KEYS) as RatingName[];

/** Short tab labels of the procedures (UX copy v19 U101, owner decision O-S4-38 (a)). */
const TAB_LABEL_KEYS: Readonly<Record<string, string>> = {
  softwareTest: 'criteria.item.detail.tabs.softwareTest',
  appTest: 'criteria.item.detail.tabs.appTest',
  pdfTest: 'criteria.item.detail.tabs.pdfTest',
};

/** The tab label of a procedure; a test type without a short label gets its full name. */
export function tabLabelKey(testType: string): string {
  return TAB_LABEL_KEYS[testType] ?? `testType.${testType}`;
}

/** The allowed ratings of a procedure text in scale order. */
export function ratingsOf(text: ProcedureText): RatingName[] {
  return RATING_ORDER.filter((rating) => text.allowedRatings.includes(rating));
}

/** The content of a section in one procedure text, as a comparable value; null when the text has none. */
function sectionValue(text: ProcedureText, kind: DetailSectionKind): string | null {
  switch (kind) {
    case 'requirement':
      return text.requirement;
    case 'applicability':
      return text.applicability;
    case 'why':
      return text.rationaleWhy;
    case 'examples':
      return text.positiveExamples === null && text.negativeExamples === null
        ? null
        : JSON.stringify([text.positiveExamples, text.negativeExamples]);
    case 'ratings': {
      const ratings = ratingsOf(text);
      return ratings.length === 0 ? null : ratings.join(' ');
    }
  }
}

/** True when the procedure text has something to show in the section. */
export function hasSection(text: ProcedureText, kind: DetailSectionKind): boolean {
  return sectionValue(text, kind) !== null;
}

/**
 * Which sections stand above the tabs and which in each tab (Handoff §5.5: „sections with
 * identical text are shown once above the tabs“). With one procedure there are no tabs and every
 * section is shared; a section with the same content in every procedure is shared; one that differs
 * or is missing in a procedure is shown in each tab that has it.
 */
export interface DetailLayout {
  /** The texts to show, in the answer's order (catalog test-type order); empty for „Geprüft nach …“. */
  procedures: ProcedureText[];
  tabbed: boolean;
  shared: DetailSectionKind[];
  perProcedure: DetailSectionKind[];
}

export function detailLayout(criterion: Criterion): DetailLayout {
  const procedures = criterion.procedureTexts;
  const shared: DetailSectionKind[] = [];
  const perProcedure: DetailSectionKind[] = [];
  for (const kind of DETAIL_SECTIONS) {
    const values = procedures.map((text) => sectionValue(text, kind));
    if (values.every((value) => value === null)) {
      continue;
    }
    if (values.every((value) => value === values[0])) {
      shared.push(kind);
    } else {
      perProcedure.push(kind);
    }
  }
  return { procedures, tabbed: procedures.length > 1, shared, perProcedure };
}
