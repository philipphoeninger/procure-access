// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/** Path of S61 „Konto“ (UX copy §4). Import-free, so `app.routes.ts` and the header can read it. */
export const ACCOUNT_PATH = 'konto';

/** The answer of `GET /api/v1/account` (SPEC §8.13): the address and whether it is confirmed. */
export interface Account {
  email: string;
  emailConfirmed: boolean;
}

/** A file the client received, as `FileSaver` takes it. */
export interface AccountExportFile {
  content: Blob;
  fileName: string;
}
