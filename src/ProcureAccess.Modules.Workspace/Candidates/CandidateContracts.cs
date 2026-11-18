// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace ProcureAccess.Modules.Workspace.Candidates;

// Bodies and answers of the candidate and evidence endpoints (SPEC R-CAN-02, R-CAN-05; S6-15).
// Internal like the endpoints. Unknown members of a body are refused, as the projects do — also
// `softwareProductId`, which comes with „Merken“ (R-CAN-01, M6).

/// <summary>
/// Body of <c>POST /api/v1/projects/{id}/candidates</c> and of <c>PATCH /api/v1/candidates/{id}</c>.
/// Only the name is required to create (owner decision S6-15 2 (a)). On a change, what is missing
/// or <c>null</c> stays, an empty text clears an optional field (the name cannot be cleared).
/// </summary>
/// <param name="Name">At most 200 characters after trimming (SPEC §7.2).</param>
/// <param name="Vendor">At most 200 characters.</param>
/// <param name="Website">An <c>https://</c> address of at most 2 048 characters.</param>
/// <param name="FunctionalSummary">At most 2 000 characters.</param>
/// <param name="ContactNotes">Private: at most 2 000 characters.</param>
/// <param name="InternalNotes">Private: at most 2 000 characters.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record CandidateBody(
    string? Name,
    string? Vendor,
    string? Website,
    string? FunctionalSummary,
    string? ContactNotes,
    string? InternalNotes);

/// <summary>Body of <c>PUT /api/v1/candidates/{id}/test-status</c>.</summary>
/// <param name="TestStatus"><c>tested</c>, <c>notTested</c> or <c>unknown</c>; required.</param>
/// <param name="RemoveEvidences">
/// <c>true</c> removes the evidences together with leaving <c>tested</c>, after the client asked
/// <c>candidate.evidence.list.statusChange</c> (owner decision S6-15 3 (a)); without it, leaving
/// <c>tested</c> with evidences answers 409 <c>test-status-conflict</c>.
/// </param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record TestStatusBody(string? TestStatus, bool? RemoveEvidences);

/// <summary>
/// Body of <c>POST /api/v1/candidates/{id}/test-evidences</c> and <c>PUT /api/v1/test-evidences/{id}</c>
/// — every field of the card, in the order of S34a (owner decision S6-15 2 (a)).
/// </summary>
/// <param name="TestKind"><c>procedure</c>, <c>other</c> („Andere Prüfung“) or <c>own</c> („Eigene Prüfung ohne festes Verfahren“); required.</param>
/// <param name="TestTypeId">The key of a catalog procedure, e.g. <c>softwareTest</c>: required with <c>procedure</c>, refused otherwise.</param>
/// <param name="OtherTestTypeName">Required with <c>other</c> (at most 120 characters), refused otherwise.</param>
/// <param name="TestedBy">„Wer hat geprüft?“, required, at most 200 characters.</param>
/// <param name="TestDate">Required, <c>yyyy-MM-dd</c>, not after today in Europe/Berlin.</param>
/// <param name="TestedVersion">At most 200 characters.</param>
/// <param name="EvidenceUrl">An <c>https://</c> address of at most 2 048 characters.</param>
/// <param name="QualificationNote">Private: at most 2 000 characters.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record TestEvidenceBody(
    string? TestKind,
    string? TestTypeId,
    string? OtherTestTypeName,
    string? TestedBy,
    DateOnly? TestDate,
    string? TestedVersion,
    string? EvidenceUrl,
    string? QualificationNote);

/// <summary>
/// A candidate as S33, S34 and S34a read it. The private section is its own branch, as the pages
/// show it apart („Nur für Ihre Organisation“); every member of the organisation reads it.
/// </summary>
/// <param name="TestStatus"><c>tested</c>, <c>notTested</c> or <c>unknown</c> (<c>candidate.evidence.tested.*</c>).</param>
/// <param name="TestEvidences">In the order they were added; empty unless <c>tested</c>.</param>
/// <param name="ModifiedAt">The last change of the facts, the private section, the test status or an evidence.</param>
internal sealed record CandidateResponse(
    Guid Id,
    Guid ProjectId,
    Guid OrganizationId,
    string Name,
    string? Vendor,
    string? Website,
    string? FunctionalSummary,
    string TestStatus,
    CandidatePrivateSectionResponse PrivateSection,
    IReadOnlyList<TestEvidenceResponse> TestEvidences,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);

/// <summary>The private section of a candidate; never published (PRV-01, PRV-02).</summary>
internal sealed record CandidatePrivateSectionResponse(string? ContactNotes, string? InternalNotes);

/// <summary>A test evidence as its card in S34a shows it, with its private note.</summary>
/// <param name="TestKind"><c>procedure</c>, <c>other</c> or <c>own</c>.</param>
/// <param name="TestTypeId">The key of the catalog procedure; <c>null</c> unless <c>procedure</c>.</param>
internal sealed record TestEvidenceResponse(
    Guid Id,
    string TestKind,
    string? TestTypeId,
    string? OtherTestTypeName,
    string TestedBy,
    DateOnly TestDate,
    string? TestedVersion,
    string? EvidenceUrl,
    string? QualificationNote);

/// <summary>A candidate in the product list of a project (S32), in the order they were added.</summary>
internal sealed record CandidateSummaryResponse(Guid Id, string Name, string? Vendor, string TestStatus);
