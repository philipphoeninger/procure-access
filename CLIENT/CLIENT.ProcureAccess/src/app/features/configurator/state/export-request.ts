// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { CriteriaSelection } from '../models/configurator-draft';
import { CriteriaResponse } from '../models/criteria';
import { CriterionDecisions } from '../models/criteria-list';
import { ExportCriterionEntry, ExportFormat, ExportRequest } from '../models/export';
import { SaveListRequest } from '../models/saved-lists';
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
 * The body of `POST /api/v1/configurations` for the list S11 shows (SPEC §8.10, R-SAV-01): the
 * draft exactly as the export sends it — the server checks it with the export's check (C153) — with
 * the organisation the header shows and the trimmed name instead of the format.
 */
export function toSaveRequest(
  organizationId: string,
  name: string,
  answer: CriteriaResponse,
  selection: CriteriaSelection,
  decisions: CriterionDecisions,
): SaveListRequest {
  const { selection: selectionBody, criteria } = toExportRequest('xlsx', answer, selection, decisions);
  return { organizationId, name, selection: selectionBody, criteria };
}
