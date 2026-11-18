// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// The procurement projects of the personal data export (SPEC R-AUTH-09, §8.4; PRV-07, workspace
/// part; S6-25): the projects a user created, with their current content. Declared here and
/// implemented by the Workspace module, as <see cref="ICriteriaListDataExport"/> is by the
/// Configurator: the export endpoint belongs to IdentityAccess, which depends on no module
/// (ARCHITECTURE §4.3).
/// </summary>
/// <remarks>
/// Read through the organisation query filter of the request (TEN-01), so the answer holds the
/// projects of the organisations the user still belongs to and is only complete for the request's
/// own user. Nothing in it carries an id or names a user: a project is exported with everything
/// in it, also what colleagues entered and the private fields, which the user can read anyway
/// (SPEC §8.4; owner decision S6-25 D1 (a)).
/// </remarks>
public interface IProjectDataExport
{
    /// <summary>The projects <paramref name="userId"/> created, oldest first.</summary>
    Task<IReadOnlyList<ExportedProject>> GetCreatedByAsync(string userId, CancellationToken cancellationToken);
}

/// <summary>A project the user created.</summary>
/// <param name="Status">As the projects API names it: <c>open</c>, <c>done</c>, <c>archived</c>.</param>
/// <param name="CriteriaList">
/// The project's own copy of a criteria list by name and release, or <c>null</c> without one. Its
/// content is in the export of the user who assigned it, among the criteria lists (owner decision
/// S6-25 D2 (a)).
/// </param>
/// <param name="Candidates">The candidate products in the order they were added.</param>
public sealed record ExportedProject(
    string Name,
    string Organization,
    string Status,
    DateTimeOffset CreatedAt,
    ExportedProjectCriteriaList? CriteriaList,
    IReadOnlyList<ExportedCandidate> Candidates);

/// <summary>The criteria list of a project: its name and the catalog release it is pinned to.</summary>
public sealed record ExportedProjectCriteriaList(string Name, string Release);

/// <summary>A candidate product with its private section and its test evidences in the order they were added.</summary>
/// <param name="TestStatus">As the candidates API names it: <c>tested</c>, <c>notTested</c>, <c>unknown</c>.</param>
public sealed record ExportedCandidate(
    string Name,
    string? Vendor,
    string? Website,
    string? FunctionalSummary,
    string TestStatus,
    string? ContactNotes,
    string? InternalNotes,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ExportedTestEvidence> TestEvidences);

/// <summary>A test evidence with its private note.</summary>
/// <param name="TestKind">As the candidates API names it: <c>procedure</c>, <c>other</c>, <c>own</c>.</param>
/// <param name="TestType">The catalog key of the procedure, e.g. <c>softwareTest</c>; <c>null</c> for the other kinds.</param>
public sealed record ExportedTestEvidence(
    string TestKind,
    string? TestType,
    string? OtherTestTypeName,
    string TestedBy,
    DateOnly TestDate,
    string? TestedVersion,
    string? EvidenceUrl,
    string? QualificationNote);
