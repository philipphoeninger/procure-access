// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { PriorityLevel } from './criteria';

/** File formats of the export (SPEC §8.3); XLSX is the default of S13 (R-LST-11). */
export type ExportFormat = 'xlsx' | 'csv';

/** One criterion of the export body: its revision and the user's decisions (S4-19, C123). */
export interface ExportCriterionEntry {
  criterionRevisionId: number;
  isIncluded: boolean;
  overrideLevel?: PriorityLevel;
  overrideWeight?: number;
  note?: string;
}

/** Body of `POST /api/v1/exports` (Export module `Requests/ExportRequest`; unknown members are refused). */
export interface ExportRequest {
  format: ExportFormat;
  selection: {
    releaseVersion: string;
    useCaseTemplateId?: string;
    appTypeIds: string[];
    productPartIds: string[];
    testTypeIds: string[];
  };
  criteria: ExportCriterionEntry[];
}

/** The exported file and the name the server gave it. */
export interface ExportFile {
  content: Blob;
  fileName: string;
}
