// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Errors;

/// <summary>
/// Names of the problem types of the API (SPEC §8.6, ARCHITECTURE §7.1, C78). The client maps each
/// type to a UX key, so a name never changes once it is in use.
/// </summary>
/// <remarks>
/// Set the name as <c>ProblemDetails.Type</c>, e.g. <c>Problem(statusCode: 409, type: ProblemTypes.Conflict)</c>;
/// Platform writes it as <c>&lt;base&gt;/problems/&lt;name&gt;</c> with the configured base URI.
/// Validation errors and 404 responses get <see cref="Validation"/> and <see cref="NotFound"/>
/// without being set.
/// </remarks>
public static class ProblemTypes
{
    public const string Validation = "validation";
    public const string Antiforgery = "antiforgery";

    /// <summary>429; carries the extension <c>retryAfterMinutes</c> (SPEC §7.3).</summary>
    public const string RateLimit = "rate-limit";

    /// <summary>409; carries the extension <c>editorDisplayName</c> (ARCHITECTURE §7.1).</summary>
    public const string Conflict = "conflict";

    public const string LockedCriteriaList = "locked-criteria-list";
    public const string TestStatusUnknown = "test-status-unknown";
    public const string NoCriteriaList = "no-criteria-list";
    public const string ProfileMismatch = "profile-mismatch";
    public const string EntryWithdrawn = "entry-withdrawn";
    public const string SelectionExcluded = "selection-excluded";
    public const string ReleaseMismatch = "release-mismatch";

    /// <summary>403 for actions that need a confirmed e-mail address (ARCHITECTURE §7.1, C72).</summary>
    public const string EmailUnconfirmed = "email-unconfirmed";

    public const string NotFound = "not-found";

    /// <summary>Every name above.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Validation, Antiforgery, RateLimit, Conflict, LockedCriteriaList, TestStatusUnknown,
        NoCriteriaList, ProfileMismatch, EntryWithdrawn, SelectionExcluded, ReleaseMismatch,
        EmailUnconfirmed, NotFound,
    ];
}
