// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Repos.Interfaces;

public interface IUserRepo
{
    Task<IdentityResult> ChangeEmailAsync(
        string userId,
        string newEmail);

    Task<IdentityResult> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword);

    Task<IdentityResult> ResetPasswordAsync(
        string email,
        string newPassword);

    Task<IdentityResult> RevokeSessionsAsync(string userId);

    Task<IdentityResult> DeleteUserAsync(string userId);
}
