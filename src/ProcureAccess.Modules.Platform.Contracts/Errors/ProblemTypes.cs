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

    /// <summary>
    /// 503: an e-mail could not be sent after its retry, while the action itself is kept (SPEC §7.6,
    /// §8.6 v2.21; S5-12a); the client shows <c>errors.email.sendFailed</c> with a retry.
    /// </summary>
    public const string EmailDelivery = "email-delivery";

    /// <summary>
    /// 409: the change would leave an organisation without an Owner — demoting, removing or the
    /// leaving of its last Owner (SPEC §8.6 v2.27, R-ORG-03, R-ORG-04; TEN-11; S5-16a); the client
    /// shows <c>org.members.lastOwner</c>.
    /// </summary>
    public const string LastOwner = "last-owner";

    /// <summary>
    /// 403: the invitation is for another e-mail address than the signed-in account's (SPEC §8.6
    /// v2.29, R-ORG-02; ARCHITECTURE C80; S5-17a); carries the extension <c>invitedEmail</c>, which the
    /// client shows in <c>org.invite.mismatch</c>.
    /// </summary>
    public const string InvitationMismatch = "invitation-mismatch";

    /// <summary>
    /// 409: the account cannot be deleted while the user is the only Owner of an organisation with
    /// other members (SPEC §8.6 v2.45, R-AUTH-10; TEN-11; S5-28a); carries the extension
    /// <c>organizations</c>, the names of these organisations, which the client shows in
    /// <c>account.delete.soleOwner</c>.
    /// </summary>
    public const string SoleOwner = "sole-owner";

    /// <summary>
    /// 409: a change of an archived procurement project that is more than a change of its status
    /// (SPEC §8.6 v2.49, R-PRJ-05; S6-11); the client shows <c>projects.archived.note</c>.
    /// </summary>
    public const string ProjectArchived = "project-archived";

    /// <summary>
    /// 409 with a field error at <c>testStatus</c>: the test status of a candidate product and its
    /// test evidences do not fit — the status is to leave „Ja“ while evidences exist, or an evidence
    /// is to be added while the status is not „Ja“ (SPEC §8.6 v2.53, R-CAN-05; WSP-01; S6-15).
    /// Carries the extension <c>evidenceCount</c>; with evidences the client asks
    /// <c>candidate.evidence.list.statusChange</c> and repeats the change with
    /// <c>removeEvidences</c>.
    /// </summary>
    public const string TestStatusConflict = "test-status-conflict";

    /// <summary>Every name above.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Validation, Antiforgery, RateLimit, Conflict, LockedCriteriaList, TestStatusUnknown,
        NoCriteriaList, ProfileMismatch, EntryWithdrawn, SelectionExcluded, ReleaseMismatch,
        EmailUnconfirmed, NotFound, EmailDelivery, LastOwner, InvitationMismatch, SoleOwner,
        ProjectArchived, TestStatusConflict,
    ];
}
