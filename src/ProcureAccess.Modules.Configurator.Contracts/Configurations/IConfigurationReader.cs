// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Configurator.Contracts.Configurations;

/// <summary>
/// How other modules read a saved criteria list (ARCHITECTURE §4.3, C62): Export for the saved
/// export (<c>PersistedConfigurationSource</c>), later Directory for its configuration-scoped
/// projection. Declared here and implemented by Configurator, so Configurator depends on neither.
/// </summary>
/// <remarks>
/// The read runs through the organisation query filter of the request (TEN-01): a list of an
/// organisation the signed-in user is no member of does not exist for the caller.
/// </remarks>
public interface IConfigurationReader
{
    /// <summary>The list with this id, or <c>null</c> when it does not exist for the request.</summary>
    Task<SavedConfiguration?> FindAsync(Guid configurationId, CancellationToken cancellationToken);
}

/// <summary>
/// A saved criteria list: its name, the catalog release it is pinned to (ADR-002), the selection by
/// the value keys of that release and one decision per criterion of the selection.
/// </summary>
public sealed record SavedConfiguration(
    Guid Id,
    string Name,
    string ReleaseVersion,
    string? UseCaseKey,
    IReadOnlyList<string> AppTypeKeys,
    IReadOnlyList<string> ProductPartKeys,
    IReadOnlyList<string> TestTypeKeys,
    IReadOnlyList<SavedCriterion> Criteria);

/// <summary>The user's decision on one criterion of a saved list, by its revision in the pinned release.</summary>
/// <param name="ResolvedLevel">The level the tool recommended when the list was saved; an override never changes it (CAT-08).</param>
/// <param name="OverrideLevel">The level the user chose instead; <c>null</c> while the recommendation applies.</param>
/// <param name="OverrideWeight">The weight the user gave; only while B applies.</param>
public sealed record SavedCriterion(
    int CriterionRevisionId,
    bool IsIncluded,
    PriorityLevel ResolvedLevel,
    PriorityLevel? OverrideLevel,
    int? OverrideWeight,
    string? Note);
