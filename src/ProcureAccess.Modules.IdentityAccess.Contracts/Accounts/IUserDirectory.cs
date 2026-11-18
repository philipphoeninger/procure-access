// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// What another module may show of a user it knows only by id (S6-10; ARCHITECTURE §4.3): the
/// display name, e.g. the competing editor of a 409 <c>conflict</c> (<c>editorDisplayName</c>,
/// SPEC §8.6; API-03).
/// </summary>
/// <remarks>
/// <para><b>The display name is the e-mail address</b> (owner decision S6-10 4): an account has no
/// other name, and the members of an organisation see each other's addresses on S21 already.</para>
/// <para><b>Who may see it is the caller's decision.</b> Nothing here checks an organisation: ask
/// only for users of a resource the request's user may read, and never put the answer into a log
/// or a public answer.</para>
/// </remarks>
public interface IUserDirectory
{
    /// <summary>
    /// The display names of <paramref name="userIds"/> in one query. A deleted or unknown account
    /// has none and is missing from the answer.
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds, CancellationToken cancellationToken);
}
