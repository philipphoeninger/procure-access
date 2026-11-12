// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Export.Sources;

/// <summary>
/// Where the content of an export comes from (ARCHITECTURE §4.4): <see cref="AdHocSelectionSource"/>
/// for the anonymous export of a draft (Flow A1, S4-19) and, from step 3, the persisted
/// configuration (Flow A2). Both hand the same <see cref="ExportContent"/> to the formatters.
/// </summary>
/// <typeparam name="TInput">What names the content: the request body, or a configuration id.</typeparam>
internal interface IExportSource<in TInput>
{
    Task<ExportSourceResult> LoadAsync(TInput input, CancellationToken cancellationToken);
}

/// <summary>Either the content or the problem that keeps the source from giving it.</summary>
internal sealed record ExportSourceResult(ExportContent? Content, ExportProblem? Problem)
{
    public static ExportSourceResult Of(ExportContent content) => new(content, null);

    public static ExportSourceResult Refused(ExportProblem problem) => new(null, problem);
}

/// <summary>
/// A refused export as Problem Details (SPEC §8.6): the status, the problem type name of
/// <see cref="ProblemTypes"/>, and for a 400 the field errors keyed by their path in the request.
/// </summary>
internal sealed record ExportProblem(int Status, string Type, string Detail, IReadOnlyDictionary<string, string[]>? Errors)
{
    public static ExportProblem Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(400, ProblemTypes.Validation, "The export request is not valid.", errors);
}
