// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Injectable } from '@angular/core';

import { InvitationDetails } from '../../models/organization';

/** What the page of an invitation link shows: the invitation with its token, „nicht mehr gültig“, or the S80 server text. */
export type InvitationView =
  | { kind: 'open'; token: string; details: InvitationDetails }
  | { kind: 'invalid' }
  | { kind: 'server' };

/** What reading the link gave: a view, or the organisation of a member (the second click, owner decision S5-17 D7 (a)). */
export type InvitationLoad = InvitationView | { kind: 'member'; organizationId: string };

/** Route data key of the {@link InvitationView}. */
export const INVITATION_VIEW_KEY = 'invitationView';

/**
 * The token of the invitation link between the guard that takes it out of the address, the
 * resolvers and the page. Memory only — never browser storage, never router state — so a reload
 * shows „Einladung nicht mehr gültig“; the link in the e-mail still works then, as on the
 * confirmation page. Unlike there the token stays until it is accepted or refused: the page reads
 * the invitation again after a sign-in in S01 over it.
 */
@Injectable({ providedIn: 'root' })
export class InvitationPageState {
  token: string | null = null;
  private loaded: { navigationId: number; result: Promise<InvitationLoad> } | null = null;

  /** One read per navigation, shared by the view and the title resolver, which run side by side. */
  loadFor(navigationId: number, read: () => Promise<InvitationLoad>): Promise<InvitationLoad> {
    if (this.loaded?.navigationId !== navigationId) {
      this.loaded = { navigationId, result: read() };
    }
    return this.loaded.result;
  }
}
