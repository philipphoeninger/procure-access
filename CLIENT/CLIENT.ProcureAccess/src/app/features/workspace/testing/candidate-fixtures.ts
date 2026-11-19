// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { CandidateAssessments } from '../models/assessments';
import { Candidate } from '../models/candidates';
import { Project } from '../models/projects';
import { CandidateCriteria, CandidateView } from '../pages/candidate-detail/candidate-load';

/** A project or a candidate as the body of an answer names it: the tag is the `ETag` header (API-03). */
export function bodyOf<T extends { etag: string }>(resource: T): Omit<T, 'etag'> {
  const { etag, ...body } = resource;
  void etag;
  return body;
}

/** A project of `org-1` with its own copy of a list, as the specs of S33 and S34 read it. */
export const TEST_PROJECT: Project = {
  id: 'p-1',
  organizationId: 'org-1',
  etag: '"1-1"',
  name: 'Neues Bewerbermanagement 2027',
  status: 'open',
  productCount: 1,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: { id: 'list-1', name: 'HR-Software 2027', releaseVersion: 'T.1' },
  criteriaListLocked: false,
  candidates: [],
};

/** A candidate of {@link TEST_PROJECT} with every fact and both private fields (Figma S34 example data). */
export const TEST_CANDIDATE: Candidate = {
  id: 'c-1',
  projectId: 'p-1',
  organizationId: 'org-1',
  etag: '"1-1"',
  name: 'BewerberPro',
  vendor: 'Beispiel Software GmbH',
  website: 'https://www.beispiel-software.de/bewerberpro',
  functionalSummary: 'Bewerbermanagement, Stellenausschreibungen, Terminplanung',
  testStatus: 'unknown',
  privateSection: {
    contactNotes: 'Frau Beispiel, Vertrieb',
    internalNotes: 'Demo am 20.10.2026 vereinbart.',
  },
  testEvidences: [],
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  progress: { done: 36, total: 46 },
  fulfillment: { fulfilled: 22, slightRestriction: 3, restriction: 6, barrier: 2, blockade: 1, notApplicable: 2, unknown: 10 },
};

export const TEST_CRITERIA: CandidateCriteria = {
  listName: 'HR-Software 2027',
  releaseVersion: 'T.1',
  appTypeNames: ['Website oder Webanwendung', 'Mobile App'],
};

/** The view S34 shows for {@link TEST_CANDIDATE}. */
export function candidateView(changes: Partial<Extract<CandidateView, { kind: 'candidate' }>> = {}): CandidateView {
  return { kind: 'candidate', project: TEST_PROJECT, candidate: TEST_CANDIDATE, criteria: TEST_CRITERIA, ...changes };
}

/**
 * The assessment tab of {@link TEST_CANDIDATE} (Figma S34b `2022:841`): three requirement criteria —
 * the first without „Nicht anwendbar“, assessed „Erfüllt“ with a note; the second „Einschränkung“;
 * the third not assessed and without a plain-language title — and two „Geprüft nach …“ rows.
 */
export const TEST_ASSESSMENTS: CandidateAssessments = {
  candidateId: 'c-1',
  releaseVersion: 'T.1',
  ratingsFromSeveralProcedures: false,
  progress: { done: 2, total: 3 },
  criteria: [
    {
      criterionRevisionId: 11,
      number: '11.2.1.1',
      title: 'Bedienbarkeit per Tastatur',
      plainLanguageTitle: 'Formulare lassen sich vollständig mit der Tastatur bedienen',
      productPart: null,
      allowedRatings: ['fulfilled', 'slightRestriction', 'restriction', 'barrier', 'blockade'],
      value: 'fulfilled',
      note: 'Eigener Test mit der Tastatur.',
    },
    {
      criterionRevisionId: 12,
      number: '11.1.4.4',
      title: 'Textgröße ändern',
      plainLanguageTitle: 'Inhalte bleiben bei 200 % Vergrößerung vollständig nutzbar',
      productPart: null,
      allowedRatings: ['fulfilled', 'slightRestriction', 'restriction', 'barrier', 'blockade', 'notApplicable'],
      value: 'restriction',
      note: null,
    },
    {
      criterionRevisionId: 13,
      number: '11.3.3.1',
      title: 'Fehlermeldungen werden dem betroffenen Eingabefeld zugeordnet und vorgelesen',
      plainLanguageTitle: null,
      productPart: null,
      allowedRatings: ['fulfilled', 'slightRestriction', 'restriction', 'barrier', 'blockade', 'notApplicable'],
      value: 'unknown',
      note: null,
    },
  ],
  procedureAnswers: [
    { criterionRevisionId: 91, testTypeId: 'softwareTest', title: 'Geprüft nach BIT inklusiv Software-Test', answer: 'yes' },
    { criterionRevisionId: 92, testTypeId: 'bikWeb', title: 'Geprüft nach BIK BITV-Test (Web)', answer: 'notStated' },
  ],
};
