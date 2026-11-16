// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { OrganizationMembers, PendingInvitation } from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';

/** What S21 shows: the members with, for an Owner, the open invitations (else null), or the S80 page in place. */
export type MembersPageView =
  | { kind: 'members'; data: OrganizationMembers; invitations: PendingInvitation[] | null }
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

/**
 * Reads the members and — when the user `ownId` is an Owner — the open invitations
 * (SPEC §8.9; only Owners may read them, owner decision S5-17 D4 (a)); a failure gives the view of
 * {@link failedView}.
 */
export async function loadMembersView(api: OrganizationApiService, organizationId: string, ownId: string | null): Promise<MembersPageView> {
  try {
    const data = await api.members(organizationId);
    const owner = data.members.some((member) => member.userId === ownId && member.role === 'owner');
    const invitations = owner ? await api.invitations(organizationId) : null;
    return { kind: 'members', data, invitations };
  } catch (error: unknown) {
    return failedView(error);
  }
}
