// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * S20's path; S21 is `<path>/:id/mitglieder` (owner decision S5-16 6a). Here, in a file without
 * imports, so `app.routes.ts` can name it without loading a page (S5-12b: no cycle through
 * `app.config.ts`).
 */
export const ORGANIZATION_PATH = 'organisation';

/** The path segment of S21 after the organisation id. */
export const MEMBERS_SEGMENT = 'mitglieder';

/** The organisation types as the API names them (SPEC §8.9); the UX copy key is `org.create.type.<key>`. */
export type OrganizationType = 'publicBody' | 'company' | 'association';

/** The organisation roles as the API names them (SPEC §8.9); the UX copy key is `org.members.role.<role>`. */
export type OrganizationRole = 'owner' | 'editor' | 'reader';

/** The types in the order S20 offers them (UX copy §5.7). */
export const ORGANIZATION_TYPES: readonly OrganizationType[] = ['publicBody', 'company', 'association'];

/** The roles in the order of the role select, highest first (UX copy §5.7). */
export const ORGANIZATION_ROLES: readonly OrganizationRole[] = ['owner', 'editor', 'reader'];

/** The UX copy key of a type: `publicBody` is `org.create.type.public`. */
export function typeKey(type: OrganizationType): string {
  return `org.create.type.${type === 'publicBody' ? 'public' : type}`;
}

/** The UX copy key of a role, e.g. `org.members.role.owner` („Verwalten“). */
export function roleKey(role: OrganizationRole): string {
  return `org.members.role.${role}`;
}

/** The body of `POST /api/v1/organizations` (SPEC §8.9). */
export interface CreateOrganizationRequest {
  name: string;
  type: OrganizationType;
}

/** The answer of `POST /api/v1/organizations`. */
export interface CreatedOrganization {
  id: string;
}

/** One member as `GET /api/v1/organizations/{id}/members` lists it; `email` is null for a deleted account. */
export interface OrganizationMember {
  userId: string;
  email: string | null;
  role: OrganizationRole;
}

/** The answer of `GET /api/v1/organizations/{id}/members`: Owners first, then Editors and Readers, each by address. */
export interface OrganizationMembers {
  organization: { id: string; name: string; type: OrganizationType };
  members: OrganizationMember[];
}
