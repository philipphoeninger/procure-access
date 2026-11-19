// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

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
