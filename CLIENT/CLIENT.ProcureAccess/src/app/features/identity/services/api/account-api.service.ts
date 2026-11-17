// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { CALLER_HANDLES_PROBLEMS } from '@app/core/errors/problem-details';
import { fileNameOf } from '@app/core/files/content-disposition';
import { Account, AccountExportFile } from '../../models/account';

/** Problem types of `DELETE /api/v1/account` that S61 shows itself (SPEC §8.13). */
export const DELETE_ACCOUNT_PROBLEMS: readonly string[] = ['sole-owner'];

/** Name used when the answer names no file (never expected: the server always sends one). */
const FALLBACK_FILE_NAME = 'ProcureAccess_Daten.json';

/** The account endpoints of S61 (SPEC §8.4, §8.13; R-AUTH-09, R-AUTH-10). */
@Injectable({ providedIn: 'root' })
export class AccountApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /** The signed-in user's address and whether it is confirmed (`GET /api/v1/account`). */
  get(): Promise<Account> {
    return lastValueFrom(this.http.get<Account>(`${this.apiUrl}/v1/account`));
  }

  /**
   * The personal data export (`GET /api/v1/account/export`, R-AUTH-09) as a file; its name comes
   * from `Content-Disposition`. A failure keeps the system states of the error interceptor.
   */
  async export(): Promise<AccountExportFile> {
    const response = await lastValueFrom(
      this.http.get(`${this.apiUrl}/v1/account/export`, { observe: 'response', responseType: 'blob' }),
    );
    return {
      content: response.body ?? new Blob(),
      fileName: fileNameOf(response.headers.get('Content-Disposition')) ?? FALLBACK_FILE_NAME,
    };
  }

  /**
   * Deletes the signed-in user's account (`DELETE /api/v1/account`, R-AUTH-10): 204 with the
   * session ended. 409 `sole-owner` with `organizations` is shown by S61
   * ({@link DELETE_ACCOUNT_PROBLEMS}); every other failure keeps the system states.
   */
  delete(): Promise<void> {
    return lastValueFrom(
      this.http.delete<void>(`${this.apiUrl}/v1/account`, {
        context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, DELETE_ACCOUNT_PROBLEMS),
      }),
    );
  }
}
