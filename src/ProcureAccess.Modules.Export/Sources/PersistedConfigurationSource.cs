// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Export.Sources;

/// <summary>
/// The content of the export of a saved list (ARCHITECTURE §4.4 Flow A2, R-LST-12): the list read
/// through <see cref="IConfigurationReader"/> — the only way Export sees a configuration (C62,
/// ARC-03) — and the texts and recommendations of the release it is pinned to from
/// <see cref="ICatalogSelectionResolver"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>Same rows as the ad-hoc source (EXP-06).</b> Both build each row with
/// <see cref="ExportRow.Of"/> from the catalog's criterion and the user's decision, in the
/// catalog's list order.</item>
/// <item><b>Pinned release (ADR-002, CAT-06).</b> The list is exported under its own release, also
/// when a newer one is active; the anonymous export accepts the active release only.</item>
/// <item><b>Not found.</b> A list that does not exist for the request — unknown, or of an
/// organisation the user is no member of (TEN-01) — is 404 <c>not-found</c>.</item>
/// </list>
/// </remarks>
internal sealed class PersistedConfigurationSource(IConfigurationReader configurations, ICatalogSelectionResolver catalog)
    : IExportSource<Guid>
{
    public async Task<ExportSourceResult> LoadAsync(Guid input, CancellationToken cancellationToken)
    {
        var list = await configurations.FindAsync(input, cancellationToken);
        if (list is null)
        {
            return ExportSourceResult.Refused(new ExportProblem(404, ProblemTypes.NotFound, "The criteria list does not exist.", null));
        }

        var resolved = await catalog.ResolveAsync(
            list.ReleaseVersion,
            new CatalogSelectionQuery(list.UseCaseKey, list.AppTypeKeys, list.ProductPartKeys, list.TestTypeKeys),
            cancellationToken);
        if (resolved.Selection is not { } selection)
        {
            // Stored releases never change (CAT-05), so a saved selection always resolves.
            throw new InvalidOperationException(
                $"The selection of saved list {list.Id} no longer resolves in release '{list.ReleaseVersion}' ({resolved.Outcome}).");
        }

        var decisions = list.Criteria.ToDictionary(c => c.CriterionRevisionId);
        var missing = selection.Criteria.Where(c => !decisions.ContainsKey(c.RevisionId)).Select(c => c.Key).ToList();
        if (missing.Count > 0 || decisions.Count != selection.Criteria.Count)
        {
            throw new InvalidOperationException(
                $"Saved list {list.Id} does not hold exactly the criteria of its selection in release '{list.ReleaseVersion}'" +
                (missing.Count > 0 ? $"; missing: {string.Join(", ", missing)}." : "."));
        }

        var rows = selection.Criteria
            .Select(c => (Criterion: c, Decision: decisions[c.RevisionId]))
            .Where(p => p.Decision.IsIncluded)
            .Select(p => ExportRow.Of(p.Criterion, p.Decision.OverrideLevel, p.Decision.OverrideWeight, p.Decision.Note))
            .ToList();
        return ExportSourceResult.Of(
            new ExportContent(list.ReleaseVersion, selection.UseCase, selection.AppTypes, selection.ProductParts, selection.TestTypes, rows),
            list.Name);
    }
}
