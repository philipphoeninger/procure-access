// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { OrganizationMembers } from '../../models/organization';

/** What S21 shows: the members, or the S80 page in place. */
export type MembersPageView =
  | { kind: 'members'; data: OrganizationMembers }
  | { kind: 'notFound' }
  | { kind: 'server' };

/** The view the resolver loaded, handed to the page; one per route (route-level provider). */
@Injectable()
export class MembersPageLoad {
  view: MembersPageView = { kind: 'notFound' };
}

/** A failed read: 404 (no such organisation or no member of it, TEN-05) and 400 (malformed id) are „not found“, the rest a server failure. */
export function failedView(error: unknown): MembersPageView {
  const status = error instanceof HttpErrorResponse ? error.status : 0;
  return status === 404 || status === 400 ? { kind: 'notFound' } : { kind: 'server' };
}
