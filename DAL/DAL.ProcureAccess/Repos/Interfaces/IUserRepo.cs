// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Repos.Interfaces;

/// <summary>
/// The legacy account deletion of the profile page (until S61 rebuilds it). The other legacy
/// account actions (change e-mail, change password, revoke sessions) went with S5-10 (owner
/// decision 2a): no screen called them.
/// </summary>
public interface IUserRepo
{
    /// <summary>
    /// Anonymises and soft-deletes the user and ends every session. Fails with the code
    /// <see cref="UserRepo.UserNotFoundCode"/> when no such user exists.
    /// </summary>
    Task<IdentityResult> DeleteUserAsync(string userId);
}
