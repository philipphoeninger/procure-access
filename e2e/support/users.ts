// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * The E2E users the API seeds at startup on the E2E stack (S5-14, TESTING §6.3;
 * `API/API.ProcureAccess/Extensions/TestUserSeedExtensions.cs`). Test data, not secrets: the
 * addresses are reserved, and the password is the `TestUsers__Password` of `docker-compose.e2e.yml`
 * — change both together (owner decision S5-14 3 (a)).
 *
 * Several tests and, locally, several browser projects share these accounts on one stack: never
 * send a wrong password for them (five lock the account, SEC-11) and never change their password
 * or address. A test that needs to change an account registers its own.
 */
export const TEST_USER_PASSWORD = 'e2e-Testkonto-Passwort';

/** Address confirmed: signs in and passes the confirmation gate. */
export const CONFIRMED_USER = 'e2e-confirmed@example.invalid';

/** Address not confirmed: signs in, but the confirmation gate refuses it (SEC-10, from S5-16). */
export const UNCONFIRMED_USER = 'e2e-unconfirmed@example.invalid';

/** Owner of {@link E2E_ORGANIZATION} (S5-16b, owner decision S5-16 7a). Never change its role: tests share it. */
export const ORG_OWNER = 'e2e-org-owner@example.invalid';

/** Editor of {@link E2E_ORGANIZATION}. */
export const ORG_EDITOR = 'e2e-org-editor@example.invalid';

/** Reader of {@link E2E_ORGANIZATION}. */
export const ORG_READER = 'e2e-org-reader@example.invalid';

/**
 * The seeded organisation with its three members; E2E tests only read it (owner decision S5-16 7a).
 * The id is fixed by the seeder (`TestUserSeedExtensions.OrganizationId`) — change both together.
 */
export const E2E_ORGANIZATION = { id: '0e2e0000-0000-4000-8000-000000000001', name: 'E2E-Organisation' } as const;
