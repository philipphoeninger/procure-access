// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/** An app type the release has no requirement criterion for (SPEC §8.2). */
export interface AppTypeWithoutCriteria {
  id: string;
  name: string;
}

/**
 * Answer of `GET /api/v1/catalog/criteria` (SPEC §8.2). S10 only needs to know that the server
 * accepted the selection (R-CON-10); the criteria themselves get their type with the S11 state (S4-14).
 */
export interface CriteriaResponse {
  release: string;
  criteria: unknown[];
  appTypesWithoutCriteria: AppTypeWithoutCriteria[];
}
