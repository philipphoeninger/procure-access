// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST, CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';
import {
  AcceptedInvitation,
  CreatedInvitation,
  CreatedOrganization,
  CreateOrganizationRequest,
  InvitationDetails,
  InviteRequest,
  OrganizationMembers,
  OrganizationRole,
  OrganizationSummary,
  PendingInvitation,
} from '../../models/organization';

/** Problem types of `POST /api/v1/organizations` that S20 shows itself (SPEC §8.9, §7.3). */
export const CREATE_PROBLEMS: readonly string[] = ['validation', 'antiforgery', 'email-unconfirmed', 'rate-limit'];

/** Problem types of a role change or a removal that S21 shows itself (SPEC §8.9). */
export const MEMBER_CHANGE_PROBLEMS: readonly string[] = ['last-owner', 'not-found', 'antiforgery'];

/** Problem types of an invitation that the invite form on S21 shows itself (SPEC §8.9; S5-17b). */
export const INVITE_PROBLEMS: readonly string[] = ['validation', 'antiforgery', 'email-delivery', 'rate-limit'];

/** Problem types of revoking an invitation that S21 shows itself. */
export const REVOKE_PROBLEMS: readonly string[] = ['not-found', 'antiforgery'];

/** Problem types of accepting an invitation that the page of the link shows itself (SPEC §8.9; SEC-10). */
export const ACCEPT_PROBLEMS: readonly string[] = ['validation', 'antiforgery', 'email-unconfirmed', 'invitation-mismatch'];

/**
 * HTTP calls of the organisation endpoints (SPEC §8.9; ARCHITECTURE C145). Each marks the problem
 * types its screen shows, so the error interceptor shows no snackbar for them.
 */
@Injectable({ providedIn: 'root' })
export class OrganizationApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /**
   * The signed-in user's organisations by name (R-SHL-02, R-SHL-03). The header loads them after the
   * session, not because the user asked: marked {@link BACKGROUND_REQUEST}, so a failure shows
   * nothing and the header stays without organisation display.
   */
  mine(): Promise<OrganizationSummary[]> {
    return lastValueFrom(
      this.http.get<OrganizationSummary[]>(`${this.apiUrl}/v1/organizations`, {
        context: new HttpContext().set(BACKGROUND_REQUEST, true),
      }),
    );
  }

  /** Creates an organisation; the signed-in user becomes its Owner (R-ORG-01). */
  create(request: CreateOrganizationRequest): Promise<CreatedOrganization> {
    return lastValueFrom(
      this.http.post<CreatedOrganization>(`${this.apiUrl}/v1/organizations`, request, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, CREATE_PROBLEMS),
      }),
    );
  }

  /**
   * The organisation and its members. Marked {@link CALLER_SHOWS_FAILURES}: S21 shows a 404 as
   * „Seite nicht gefunden“ and any other failure as the S80 server text in place.
   */
  members(organizationId: string): Promise<OrganizationMembers> {
    return lastValueFrom(
      this.http.get<OrganizationMembers>(`${this.apiUrl}/v1/organizations/${encodeURIComponent(organizationId)}/members`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /** Gives a member another role (Owner only; 409 `last-owner` for the last Owner, R-ORG-03). */
  changeRole(organizationId: string, userId: string, role: OrganizationRole): Promise<void> {
    return lastValueFrom(
      this.http.patch<void>(this.memberUrl(organizationId, userId), { role }, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, MEMBER_CHANGE_PROBLEMS),
      }),
    );
  }

  /** Removes a member, or with the own user id leaves the organisation (R-ORG-03, R-ORG-04). */
  remove(organizationId: string, userId: string): Promise<void> {
    return lastValueFrom(
      this.http.delete<void>(this.memberUrl(organizationId, userId), {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, MEMBER_CHANGE_PROBLEMS),
      }),
    );
  }

  /** Invites an address with a role (Owner only, R-ORG-02); replaces an open invitation of the same address. */
  invite(organizationId: string, request: InviteRequest): Promise<CreatedInvitation> {
    return lastValueFrom(
      this.http.post<CreatedInvitation>(this.invitationsUrl(organizationId), request, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, INVITE_PROBLEMS),
      }),
    );
  }

  /** The open invitations (Owner only); marked {@link CALLER_SHOWS_FAILURES} like {@link members}. */
  invitations(organizationId: string): Promise<PendingInvitation[]> {
    return lastValueFrom(
      this.http.get<PendingInvitation[]>(this.invitationsUrl(organizationId), {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /** Revokes an open invitation (Owner only, R-ORG-03); 404 when it is no longer open. */
  revokeInvitation(organizationId: string, invitationId: string): Promise<void> {
    return lastValueFrom(
      this.http.delete<void>(`${this.invitationsUrl(organizationId)}/${encodeURIComponent(invitationId)}`, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, REVOKE_PROBLEMS),
      }),
    );
  }

  /**
   * What the token of an invitation link invites to, also signed out (R-ORG-02). Marked
   * {@link CALLER_SHOWS_FAILURES}: the page shows 400 as „Einladung nicht mehr gültig“ and any other
   * failure as the S80 server text.
   */
  viewInvitation(token: string): Promise<InvitationDetails> {
    return lastValueFrom(
      this.http.post<InvitationDetails>(`${this.apiUrl}/v1/invitations/view`, { token }, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /** Accepts an invitation for the signed-in user (confirmed address, invited address; R-ORG-02, SEC-10). */
  acceptInvitation(token: string): Promise<AcceptedInvitation> {
    return lastValueFrom(
      this.http.post<AcceptedInvitation>(`${this.apiUrl}/v1/invitations/accept`, { token }, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, ACCEPT_PROBLEMS),
      }),
    );
  }

  private invitationsUrl(organizationId: string): string {
    return `${this.apiUrl}/v1/organizations/${encodeURIComponent(organizationId)}/invitations`;
  }

  private memberUrl(organizationId: string, userId: string): string {
    return `${this.apiUrl}/v1/organizations/${encodeURIComponent(organizationId)}/members/${encodeURIComponent(userId)}`;
  }
}
