// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';
import {
  CreatedOrganization,
  CreateOrganizationRequest,
  OrganizationMembers,
  OrganizationRole,
} from '../../models/organization';

/** Problem types of `POST /api/v1/organizations` that S20 shows itself (SPEC §8.9). */
export const CREATE_PROBLEMS: readonly string[] = ['validation', 'antiforgery', 'email-unconfirmed'];

/** Problem types of a role change or a removal that S21 shows itself (SPEC §8.9). */
export const MEMBER_CHANGE_PROBLEMS: readonly string[] = ['last-owner', 'not-found', 'antiforgery'];

/**
 * HTTP calls of the organisation endpoints (SPEC §8.9; ARCHITECTURE C145). Each marks the problem
 * types its screen shows, so the error interceptor shows no snackbar for them.
 */
@Injectable({ providedIn: 'root' })
export class OrganizationApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

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

  private memberUrl(organizationId: string, userId: string): string {
    return `${this.apiUrl}/v1/organizations/${encodeURIComponent(organizationId)}/members/${encodeURIComponent(userId)}`;
  }
}
