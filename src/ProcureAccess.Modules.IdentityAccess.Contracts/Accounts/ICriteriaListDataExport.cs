// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// The criteria lists of the personal data export (SPEC R-AUTH-09, §8.4; ARCHITECTURE C151;
/// PRV-07; S5-19): the lists a user created, with their current content. Declared here and
/// implemented by the Configurator module (S6-10): the export endpoint
/// <c>GET /api/v1/account/export</c> belongs to IdentityAccess, which depends on no module
/// (ARCHITECTURE §4.3), so the module that owns the data supplies its section.
/// </summary>
/// <remarks>
/// Read through the organisation query filter of the request (TEN-01), so the answer holds the
/// lists of the organisations the user still belongs to and is only complete for the request's own
/// user (owner decision S5-19 4). Nothing in it carries an id or names another user.
/// </remarks>
public interface ICriteriaListDataExport
{
    /// <summary>The lists <paramref name="userId"/> created, oldest first.</summary>
    Task<IReadOnlyList<ExportedCriteriaList>> GetCreatedByAsync(string userId, CancellationToken cancellationToken);
}

/// <summary>A list the user created: name, organisation name, release, creation time, selection and criteria in list order.</summary>
public sealed record ExportedCriteriaList(
    string Name,
    string Organization,
    string Release,
    DateTimeOffset CreatedAt,
    ExportedListSelection Selection,
    IReadOnlyList<ExportedListCriterion> Criteria);

/// <summary>The selection of a list by the German names of its values.</summary>
public sealed record ExportedListSelection(
    string? UseCase,
    IReadOnlyList<string> AppTypes,
    IReadOnlyList<string> ProductParts,
    IReadOnlyList<string> TestTypes);

/// <summary>One criterion of a list with the user's decision.</summary>
/// <param name="Number">The EN 301 549 number; <c>null</c> for a „Geprüft nach …“ criterion.</param>
/// <param name="RecommendedLevel">The level the tool recommended when the list was saved, as SPEC §8.2 names it: <c>A</c>, <c>B</c>, <c>Info</c>.</param>
/// <param name="EffectiveLevel">The level that applies: the user's choice, else the recommendation; named like <paramref name="RecommendedLevel"/>.</param>
/// <param name="Weight">The weight that applies, only while B applies.</param>
public sealed record ExportedListCriterion(
    string? Number,
    string Title,
    bool IsIncluded,
    string RecommendedLevel,
    string EffectiveLevel,
    int? Weight,
    string? Note);
