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
