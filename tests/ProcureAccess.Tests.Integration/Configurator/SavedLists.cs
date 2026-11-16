// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;
using Shouldly;

namespace ProcureAccess.Tests.Integration.Configurator;

/// <summary>
/// Saved criteria lists for tests (S5-19): a selection of a test release as the host's catalog
/// resolves it, and a list built from it as the save path of S5-20 will build one — one row per
/// criterion of the selection with the recommended level of the catalog.
/// </summary>
internal static class SavedLists
{
    /// <summary>The selection of <c>ExportTestCatalog.WebSelection</c>: a use case, a procedure and a „Geprüft nach …“ criterion.</summary>
    public static readonly CatalogSelectionQuery WebSelection = new("hr", ["web"], ["base"], ["softwareTest"]);

    /// <summary>Resolves <see cref="WebSelection"/> under <paramref name="release"/> with the host's catalog.</summary>
    public static async Task<ResolvedCatalogSelection> ResolveAsync(
        IServiceProvider services, string release, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var resolved = await scope.ServiceProvider.GetRequiredService<ICatalogSelectionResolver>()
                                  .ResolveAsync(release, WebSelection, cancellationToken);
        resolved.Outcome.ShouldBe(CatalogSelectionOutcome.Resolved);
        return resolved.Selection.ShouldNotBeNull();
    }

    /// <summary>
    /// A list of <paramref name="organizationId"/> with every criterion of <paramref name="selection"/>;
    /// <paramref name="decide"/> gives the decision of single criteria, the others are included unchanged.
    /// </summary>
    public static CriteriaConfiguration Build(
        Guid organizationId,
        string createdByUserId,
        string name,
        DateTimeOffset createdAt,
        string release,
        ResolvedCatalogSelection selection,
        Func<ResolvedCatalogCriterion, ConfigurationCriterionDraft?>? decide = null) =>
        CriteriaConfiguration.Create(
            organizationId,
            name,
            createdByUserId,
            createdAt,
            release,
            selection.UseCase?.Key,
            [.. selection.AppTypes.Select(v => v.Key)],
            [.. selection.ProductParts.Select(v => v.Key)],
            [.. selection.TestTypes.Select(v => v.Key)],
            [.. selection.Criteria.Select(c => decide?.Invoke(c) ?? Draft(c))]);

    /// <summary>The row of <paramref name="criterion"/> with its recommended level and the given decision.</summary>
    public static ConfigurationCriterionDraft Draft(
        ResolvedCatalogCriterion criterion,
        bool isIncluded = true,
        PriorityLevel? overrideLevel = null,
        int? overrideWeight = null,
        string? note = null) =>
        new(criterion.RevisionId, criterion.RecommendedPriority.Level, isIncluded, overrideLevel, overrideWeight, note);
}
