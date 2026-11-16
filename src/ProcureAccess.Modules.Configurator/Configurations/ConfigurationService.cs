// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Persistence;

namespace ProcureAccess.Modules.Configurator.Configurations;

/// <summary>
/// Saves, reads and lists the organisation's criteria lists (R-SAV-01, R-SAV-02; S5-20a) for
/// <see cref="ConfigurationEndpoints"/>. Every read runs through the organisation filter of the
/// request (TEN-01); the endpoints check the role on the named organisation before they call it.
/// </summary>
internal sealed class ConfigurationService(ConfiguratorDbContext db, IConfigurationDraftCheck drafts, TimeProvider clock)
{
    /// <summary>The draft check for a new list: the export's rules, and an inactive use case is refused (CAT-18).</summary>
    public Task<DraftCheckResult> CheckAsync(CriteriaDraft draft, CancellationToken cancellationToken) =>
        drafts.CheckAsync(draft, DraftPurpose.Save, cancellationToken);

    /// <summary>
    /// Stores a checked draft as a new list of <paramref name="organizationId"/> and returns its id.
    /// The recommended level of each criterion is the one the catalog resolved now, never the
    /// client's (EXP-07); it stays as it is from here on (CAT-08).
    /// </summary>
    public async Task<Guid> CreateAsync(
        Guid organizationId, string name, string createdByUserId, CheckedDraft draft, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var list = CriteriaConfiguration.Create(
            organizationId,
            name,
            createdByUserId,
            clock.GetUtcNow(),
            draft.ReleaseVersion,
            draft.Selection.UseCase?.Key,
            [.. draft.Selection.AppTypes.Select(v => v.Key)],
            [.. draft.Selection.ProductParts.Select(v => v.Key)],
            [.. draft.Selection.TestTypes.Select(v => v.Key)],
            [.. draft.Criteria.Select(c => new ConfigurationCriterionDraft(
                c.Criterion.RevisionId,
                c.Criterion.RecommendedPriority.Level,
                c.IsIncluded,
                c.OverrideLevel,
                c.OverrideWeight,
                c.Note))]);
        db.CriteriaConfigurations.Add(list);
        await db.SaveChangesAsync(cancellationToken);
        return list.Id;
    }

    /// <summary>
    /// The list with this id, or <c>null</c> when it does not exist for the request (also another
    /// organisation's: 404, TEN-01). The decisions come by revision id; the order of the list is the
    /// catalog's, which S11 takes from the criteria endpoint of the pinned release.
    /// </summary>
    public async Task<ConfigurationResponse?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var list = await db.CriteriaConfigurations.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        return list is null
            ? null
            : new ConfigurationResponse(
                list.Id,
                list.OrganizationId,
                list.Name,
                list.ReleaseVersion,
                list.CreatedAt,
                list.ModifiedAt,
                new ConfigurationSelectionResponse(
                    list.UseCaseKey,
                    [.. list.AppTypes.Select(v => v.AppTypeKey)],
                    [.. list.ProductParts.Select(v => v.ProductPartKey)],
                    [.. list.TestTypes.Select(v => v.TestTypeKey)]),
                [.. list.Criteria.OrderBy(c => c.CriterionRevisionId).Select(c => new ConfigurationCriterionResponse(
                    c.CriterionRevisionId,
                    c.IsIncluded,
                    LevelName(c.ResolvedLevel),
                    c.OverrideLevel is { } level ? LevelName(level) : null,
                    c.OverrideWeight,
                    c.NoteDe))]);
    }

    /// <summary>
    /// The lists of <paramref name="organizationId"/>, the most recently changed first; the client
    /// sorts them (S14). Only the columns of the list, none of its rows.
    /// </summary>
    public async Task<IReadOnlyList<ConfigurationSummaryResponse>> ListAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await db.CriteriaConfigurations.AsNoTracking()
                .Where(c => c.OrganizationId == organizationId)
                .OrderByDescending(c => c.ModifiedAt)
                .ThenBy(c => c.Id)
                .Select(c => new ConfigurationSummaryResponse(c.Id, c.Name, c.ReleaseVersion, c.CreatedAt, c.ModifiedAt))
                .ToListAsync(cancellationToken);

    /// <summary>The level names of SPEC §8.2 and the UX keys.</summary>
    internal static string LevelName(PriorityLevel level) => level switch
    {
        PriorityLevel.A => "A",
        PriorityLevel.B => "B",
        PriorityLevel.Info => "Info",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown level."),
    };
}
