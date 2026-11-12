// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { CriteriaSelection } from '../models/configurator-draft';
import { CriteriaResponse } from '../models/criteria';
import { CriterionDecisions } from '../models/criteria-list';
import { ExportCriterionEntry, ExportFormat, ExportRequest } from '../models/export';
import { decisionOf, effectiveLevel } from './criteria-list-rules';

/**
 * The body of `POST /api/v1/exports` for the list S11 shows (SPEC §8.3, ARCHITECTURE §7.2; S4-19
 * C123): the release of the answer, the selection by key and every criterion of the answer once by
 * revision with the user's decisions. The server re-resolves every priority (EXP-07), so no
 * recommended level is sent: only an override, a weight while B applies and a note that is not empty.
 */
export function toExportRequest(
  format: ExportFormat,
  answer: CriteriaResponse,
  selection: CriteriaSelection,
  decisions: CriterionDecisions,
): ExportRequest {
  return {
    format,
    selection: {
      releaseVersion: answer.release,
      ...(selection.useCase === undefined ? {} : { useCaseTemplateId: selection.useCase }),
      appTypeIds: [...selection.appTypeIds],
      productPartIds: [...selection.productPartIds],
      testTypeIds: [...selection.testTypeIds],
    },
    criteria: answer.criteria.map((criterion): ExportCriterionEntry => {
      const decision = decisionOf(decisions, criterion.key);
      const entry: ExportCriterionEntry = { criterionRevisionId: criterion.revisionId, isIncluded: decision.included };
      if (decision.overrideLevel !== null) entry.overrideLevel = decision.overrideLevel;
      if (decision.overrideWeight !== null && effectiveLevel(criterion, decision) === 'B') {
        entry.overrideWeight = decision.overrideWeight;
      }
      if (decision.note !== '') entry.note = decision.note;
      return entry;
    }),
  };
}

/**
 * The file name of a `Content-Disposition` header: `filename*` (RFC 6266, UTF-8, as ASP.NET writes
 * it) before `filename`; null when the header names none.
 */
export function fileNameOf(contentDisposition: string | null): string | null {
  if (contentDisposition === null) return null;
  const extended = /filename\*\s*=\s*UTF-8''([^;]+)/i.exec(contentDisposition);
  if (extended) {
    try {
      return decodeURIComponent(extended[1].trim());
    } catch {
      // A malformed escape: fall back to the plain name.
    }
  }
  const plain = /filename\s*=\s*"?([^";]+)"?/i.exec(contentDisposition);
  return plain ? plain[1].trim() : null;
}
