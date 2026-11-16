// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Requests;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Export.Sources;

/// <summary>
/// The content of the anonymous export (ARCHITECTURE §4.4 Flow A1, R-LST-12): the client's draft,
/// checked and re-resolved on the server by <see cref="IConfigurationDraftCheck"/> — the check that
/// also guards saving a list (C153), so a draft the export accepts can be saved and the other way
/// round. It reads no saved configuration and writes nothing, so the export persists nothing
/// (EXP-05; ARC-03: of <c>Configurator.Contracts</c> it uses only the draft check).
/// </summary>
/// <remarks>
/// The rules — server-resolved priorities (EXP-07), the active release only (O-S4-60 (a)), the
/// selection check (CAT-11, CAT-12), every criterion once (O-S4-57 (a)), the limits of SPEC §7.2 —
/// are described on the check. Here: its outcome as Problem Details of the export, and the rows of
/// the included criteria in list order. An inactive use case is accepted (CAT-18; owner, S3-20).
/// </remarks>
internal sealed class AdHocSelectionSource(IConfigurationDraftCheck drafts) : IExportSource<ExportRequest>
{
    public async Task<ExportSourceResult> LoadAsync(ExportRequest input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var result = await drafts.CheckAsync(DraftOf(input), DraftPurpose.Export, cancellationToken);
        return result.Outcome switch
        {
            DraftCheckOutcome.Checked => ExportSourceResult.Of(ContentOf(result.Draft!)),
            DraftCheckOutcome.Invalid => Refused(ExportProblem.Validation(result.Errors)),
            DraftCheckOutcome.SelectionExcluded => Refused(new ExportProblem(400, ProblemTypes.SelectionExcluded, result.Detail, result.Errors)),
            DraftCheckOutcome.ReleaseNotFound => Refused(new ExportProblem(404, ProblemTypes.NotFound, result.Detail, null)),
            DraftCheckOutcome.ReleaseNotActive => Refused(new ExportProblem(400, ProblemTypes.ReleaseMismatch, result.Detail, result.Errors)),
            _ => throw new InvalidOperationException($"Unknown outcome {result.Outcome} of the draft check."),
        };
    }

    private static CriteriaDraft DraftOf(ExportRequest request) => new(
        request.Selection is { } s
            ? new DraftSelection(s.ReleaseVersion, s.UseCaseTemplateId, s.AppTypeIds, s.ProductPartIds, s.TestTypeIds)
            : null,
        request.Criteria?.Select(c => c is null
            ? null
            : new DraftCriterion(c.CriterionRevisionId, c.IsIncluded, c.OverrideLevel, c.OverrideWeight, c.Note)).ToList());

    private static ExportContent ContentOf(CheckedDraft draft) => new(
        draft.ReleaseVersion,
        draft.Selection.UseCase,
        draft.Selection.AppTypes,
        draft.Selection.ProductParts,
        draft.Selection.TestTypes,
        [.. draft.Criteria.Where(c => c.IsIncluded).Select(c => ExportRow.Of(c.Criterion, c.OverrideLevel, c.OverrideWeight, c.Note))]);

    private static ExportSourceResult Refused(ExportProblem problem) => ExportSourceResult.Refused(problem);
}
