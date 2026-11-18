// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Candidate products of a procurement project (SPEC R-CAN-02 – R-CAN-05; ARCHITECTURE §7.2;
// S6-15 server, S6-16 client). Types and route paths only, importing nothing but `projects.ts`, so the
// routing files can name the paths without loading a store or service (CLAUDE.md §3).

import { projectPath } from './projects';

/** The segment of the candidates below a project's S32 address (`/projekte/:id/produkte`). */
export const CANDIDATES_SEGMENT = 'produkte';

/** S33 „Produkt hinzufügen“ below the candidates of a project (UX copy §3: `/projekte/:id/produkte/neu`). */
export const NEW_CANDIDATE_SEGMENT = 'neu';

/** S33 „Produkt bearbeiten“ below a candidate's S34 address (the UX copy names only the address of a new one). */
export const EDIT_CANDIDATE_SEGMENT = 'bearbeiten';

/**
 * The tabs of S34 as routes (R-CAN-03, Handoff §5.1): Überblick without a suffix, then
 * Prüfnachweis and Einschätzung nach Kriterien. The UX copy key of a tab is `candidate.tabs.<tab>`.
 */
export const CANDIDATE_TABS = ['overview', 'evidence', 'assessment'] as const;
export type CandidateTab = (typeof CANDIDATE_TABS)[number];

/** The route segment of each tab (Figma mapping §2: child routes `pruefnachweis`, `einschaetzung`). */
export const CANDIDATE_TAB_SEGMENTS: Readonly<Record<CandidateTab, string>> = {
  overview: '',
  evidence: 'pruefnachweis',
  assessment: 'einschaetzung',
};

/** The address of S33 for a new candidate of the project. */
export function newCandidatePath(projectId: string): string {
  return `${projectPath(projectId)}/${CANDIDATES_SEGMENT}/${NEW_CANDIDATE_SEGMENT}`;
}

/** The address of a candidate's S34, on the given tab (Überblick by default). */
export function candidatePath(projectId: string, candidateId: string, tab: CandidateTab = 'overview'): string {
  const base = `${projectPath(projectId)}/${CANDIDATES_SEGMENT}/${candidateId}`;
  const segment = CANDIDATE_TAB_SEGMENTS[tab];
  return segment === '' ? base : `${base}/${segment}`;
}

/** The address of S33 for an existing candidate. */
export function editCandidatePath(projectId: string, candidateId: string): string {
  return `${candidatePath(projectId, candidateId)}/${EDIT_CANDIDATE_SEGMENT}`;
}

/**
 * The field limits of S33 as the server checks them (SPEC §7.2; `CandidateProduct`,
 * `CandidatePrivateSection`), counted like its `string.Length` after trimming.
 */
export const CANDIDATE_LIMITS = {
  name: 200,
  vendor: 200,
  website: 2048,
  functionalSummary: 2000,
  contactNotes: 2000,
  internalNotes: 2000,
} as const;

/** The test status as the API names it (R-CAN-05); the UX copy key is `candidate.evidence.tested.<key>`. */
export type TestStatus = 'tested' | 'notTested' | 'unknown';

/** The UX copy key of a test status: „Ja“, „Nein“ or „Nicht bekannt“. */
export function testStatusKey(status: TestStatus): string {
  const key = { tested: 'yes', notTested: 'no', unknown: 'unknown' } as const;
  return `candidate.evidence.tested.${key[status]}`;
}

/** The kind of a test evidence: a catalog procedure, „Andere Prüfung“ or „Eigene Prüfung ohne festes Verfahren“. */
export type TestKind = 'procedure' | 'other' | 'own';

/** A test evidence as its card in S34a shows it (S6-17 builds the card), with its private note. */
export interface TestEvidence {
  id: string;
  testKind: TestKind;
  /** The key of the catalog procedure (`testType.<key>`); null unless `procedure`. */
  testTypeId: string | null;
  /** The name the user gave „Andere Prüfung“; null unless `other`. */
  otherTestTypeName: string | null;
  testedBy: string;
  testDate: string;
  testedVersion: string | null;
  evidenceUrl: string | null;
  /** Private (PRV-01): never published. */
  qualificationNote: string | null;
  /** The tag of the evidence for `If-Match` when it is changed or removed (API-03, S6-20); a member of the body. */
  etag: string;
}

/** „Nur für Ihre Organisation“: never published (PRV-01, PRV-02). */
export interface CandidatePrivateSection {
  contactNotes: string | null;
  internalNotes: string | null;
}

/** How far the assessment is (R-CAN-08): requirement criteria only; a measure of the work, not of the product. */
export interface AssessmentProgress {
  done: number;
  total: number;
}

/**
 * K1 in the workspace scope as the server counts it (S6-18; SIG-13): the included requirement
 * criteria of the project's list by value — always these seven, never `notCovered`, never a total.
 */
export interface CandidateFulfillment {
  fulfilled: number;
  slightRestriction: number;
  restriction: number;
  barrier: number;
  blockade: number;
  notApplicable: number;
  unknown: number;
}

/** A candidate (`GET /api/v1/candidates/{id}` and the answers of creating and changing it). */
export interface Candidate {
  id: string;
  /** The `ETag` of the answer, sent back in `If-Match` with a change (API-03, S6-20); not a member of the body. */
  etag: string;
  projectId: string;
  organizationId: string;
  name: string;
  vendor: string | null;
  website: string | null;
  functionalSummary: string | null;
  testStatus: TestStatus;
  privateSection: CandidatePrivateSection;
  /** In the order they were added; empty unless `tested`. */
  testEvidences: TestEvidence[];
  createdAt: string;
  modifiedAt: string;
  /** Null while the project has no list (S6-18). */
  progress: AssessmentProgress | null;
  /** Null while the project has no list (S6-18). */
  fulfillment: CandidateFulfillment | null;
}

/**
 * Body of creating (`POST /api/v1/projects/{id}/candidates`) and changing
 * (`PATCH /api/v1/candidates/{id}`) a candidate. S33 always sends every field: an empty text clears
 * an optional field on a change (S6-15).
 */
export interface CandidateRequest {
  name: string;
  vendor: string;
  website: string;
  functionalSummary: string;
  contactNotes: string;
  internalNotes: string;
}
