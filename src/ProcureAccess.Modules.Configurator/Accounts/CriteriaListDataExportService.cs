// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

namespace ProcureAccess.Modules.Configurator.Accounts;

/// <inheritdoc cref="ICriteriaListDataExport"/>
/// <remarks>
/// The names of the selection, the numbers and the titles come from the catalog release the list is
/// pinned to (<see cref="ICatalogSelectionResolver"/>), the organisation names from the user's
/// memberships (<see cref="IOrganizationMembers"/>). The recommended level is the stored one of the
/// list (CAT-08); the recommended weight is the catalog's.
/// </remarks>
internal sealed class CriteriaListDataExportService(
    ConfiguratorDbContext db, ICatalogSelectionResolver catalog, IOrganizationMembers organizations)
    : ICriteriaListDataExport
{
    public async Task<IReadOnlyList<ExportedCriteriaList>> GetCreatedByAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // Through the organisation filter of the request (TEN-01): the lists of the organisations
        // the signed-in user still belongs to (owner decision S5-19 4).
        var lists = await db.CriteriaConfigurations.AsNoTracking()
                            .Where(c => c.CreatedByUserId == userId)
                            .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
                            .ToListAsync(cancellationToken);
        if (lists.Count == 0)
        {
            return [];
        }

        var organizationNames = (await organizations.ListOfAsync(userId, cancellationToken)).ToDictionary(o => o.Id, o => o.Name);
        var exported = new List<ExportedCriteriaList>(lists.Count);
        foreach (var list in lists)
        {
            // A membership removed while this request runs: the list is no longer the user's to read.
            if (organizationNames.TryGetValue(list.OrganizationId, out var organization))
            {
                exported.Add(await ExportAsync(list, organization, cancellationToken));
            }
        }

        return exported;
    }

    private async Task<ExportedCriteriaList> ExportAsync(CriteriaConfiguration list, string organization, CancellationToken cancellationToken)
    {
        var resolved = await catalog.ResolveAsync(
            list.ReleaseVersion,
            new CatalogSelectionQuery(
                list.UseCaseKey,
                [.. list.AppTypes.Select(v => v.AppTypeKey)],
                [.. list.ProductParts.Select(v => v.ProductPartKey)],
                [.. list.TestTypes.Select(v => v.TestTypeKey)]),
            cancellationToken);
        if (resolved.Selection is not { } selection)
        {
            // Stored releases never change (CAT-05), so a saved selection always resolves.
            throw new InvalidOperationException(
                $"The selection of a saved list no longer resolves in release '{list.ReleaseVersion}' ({resolved.Outcome}).");
        }

        var decisions = list.Criteria.ToDictionary(c => c.CriterionRevisionId);
        return new ExportedCriteriaList(
            list.Name,
            organization,
            list.ReleaseVersion,
            list.CreatedAt,
            new ExportedListSelection(
                selection.UseCase?.Name,
                [.. selection.AppTypes.Select(v => v.Name)],
                [.. selection.ProductParts.Select(v => v.Name)],
                [.. selection.TestTypes.Select(v => v.Name)]),
            [.. selection.Criteria.Where(c => decisions.ContainsKey(c.RevisionId))
                         .Select(c => CriterionOf(c, decisions[c.RevisionId]))]);
    }

    /// <summary>The level names of SPEC §8.2: <c>A</c>, <c>B</c>, <c>Info</c>.</summary>
    private static string LevelName(PriorityLevel level) => level switch
    {
        PriorityLevel.A => "A",
        PriorityLevel.B => "B",
        PriorityLevel.Info => "Info",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown priority level."),
    };

    private static ExportedListCriterion CriterionOf(ResolvedCatalogCriterion criterion, ConfigurationCriterion decision)
    {
        var effective = decision.EffectiveLevel;
        int? weight = effective != PriorityLevel.B
            ? null
            : decision.OverrideWeight ?? (decision.ResolvedLevel == PriorityLevel.B ? criterion.RecommendedPriority.Weight : null);
        return new ExportedListCriterion(
            criterion.Number,
            criterion.PlainLanguageTitle ?? criterion.Title,
            decision.IsIncluded,
            LevelName(decision.ResolvedLevel),
            LevelName(effective),
            weight,
            decision.NoteDe);
    }
}
