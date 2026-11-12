// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { ExportFile, ExportRequest } from '../../models/export';
import { fileNameOf } from '../../state/export-request';

/** Name used when the answer names no file (never expected: the server always sends one). */
const FALLBACK_FILE_NAME = 'ProcureAccess_Kriterien';

@Injectable({ providedIn: 'root' })
export class ExportApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /**
   * Asks `POST /api/v1/exports` for the file (R-LST-11, R-LST-12). Marked
   * {@link CALLER_SHOWS_FAILURES}: the export dialog S13 shows every failure itself (O-S4-74).
   * The file name comes from `Content-Disposition` (SPEC §8.3), which the API exposes to CORS.
   */
  async exportList(request: ExportRequest): Promise<ExportFile> {
    const response = await lastValueFrom(
      this.http.post(`${this.apiUrl}/v1/exports`, request, {
        observe: 'response',
        responseType: 'blob',
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
    return {
      content: response.body ?? new Blob(),
      fileName:
        fileNameOf(response.headers.get('Content-Disposition')) ?? `${FALLBACK_FILE_NAME}.${request.format}`,
    };
  }
}
