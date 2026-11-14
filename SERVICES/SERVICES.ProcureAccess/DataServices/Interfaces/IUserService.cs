// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IUserService
{
    Task RequestPasswordResetAsync(string email);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
    Task<UserDto?> GetCurrentUserAsync();
}
