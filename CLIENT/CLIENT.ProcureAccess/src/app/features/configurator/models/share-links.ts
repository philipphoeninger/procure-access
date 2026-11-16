// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { SavedList } from './saved-lists';

// Share links of saved lists (SPEC R-SAV-04, R-SAV-05, §8.5, §8.10; S5-23, S5-24). Types only and
// the route path, so app.routes.ts can import the path without loading a store or service
// (CLAUDE.md §3, client routing).

/** S16 „Geteilte Kriterienliste“ at `/geteilt/:token`, readable without an account (TEN-09). */
export const SHARED_PATH = 'geteilt';

/**
 * The link to hand on: this client's origin and `/geteilt/<token>` (SPEC §8.10). The token stays
 * in the path (owner decision S5-23 1a); the referrer policy `strict-origin` keeps it out of the
 * requests the page makes.
 */
export function sharedListUrl(origin: string, token: string): string {
  return `${origin}/${SHARED_PATH}/${token}`;
}

/** The last days a new link may have today, worked out by the server in its time zone (`yyyy-MM-dd`; owner decision S5-24 1a). */
export interface ShareLinkExpiry {
  earliest: string;
  default: string;
  latest: string;
}

/** An active link of S15: never with its token, which the server keeps only as a hash. */
export interface ActiveShareLink {
  id: string;
  createdAt: string;
  expiresAt: string;
  /** The last day the link works (`yyyy-MM-dd`), as it was chosen. */
  expiresOn: string;
}

/** The answer of `GET /api/v1/configurations/{id}/share-links`. */
export interface ShareLinksResponse {
  expiry: ShareLinkExpiry;
  links: ActiveShareLink[];
}

/** The answer of `POST /api/v1/configurations/{id}/share-links`: the only one that holds the token. */
export interface CreatedShareLink {
  id: string;
  token: string;
  expiresAt: string;
}

/**
 * The answer of `GET /api/v1/shared/{token}` (SPEC §8.5): the list without its ids, organisation
 * and dates, plus `expiresAt`; the texts come from the criteria of its release, as for a saved list.
 */
export type SharedList = Pick<SavedList, 'name' | 'releaseVersion' | 'selection' | 'criteria'> & { expiresAt: string };
