// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Auth;

/// <summary>
/// The password rule of SPEC R-AUTH-01 and ARCHITECTURE §9.1 (C72; S5-12a): at least
/// 🔧 <c>Account:Password:MinLength</c> (12) characters, at most <see cref="MaxLength"/>, no
/// composition rules. Set as Identity's options, so every place that sets a password follows it:
/// registration, and the legacy password change and reset until S5-13 replaces them.
/// </summary>
/// <remarks>Lengths count UTF-16 code units, as <see cref="string.Length"/> and Identity do.</remarks>
public static class PasswordPolicy
{
    public const string MinLengthSetting = "Account:Password:MinLength";

    /// <summary>Longest password (SPEC R-AUTH-01); not a 🔧 setting.</summary>
    public const int MaxLength = 128;

    /// <summary>Reads the 🔧 minimum length; a missing or impossible value stops the start.</summary>
    public static int MinLengthFrom(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var minLength = configuration.GetValue<int?>(MinLengthSetting);
        if (minLength is null or < 1 or > MaxLength)
        {
            throw new InvalidOperationException(
                $"{MinLengthSetting} must be a whole number from 1 to {MaxLength} (SPEC R-AUTH-01: 12).");
        }

        return minLength.Value;
    }
}

/// <summary>Refuses a password longer than <see cref="PasswordPolicy.MaxLength"/>; Identity has no such option.</summary>
public sealed class MaximumLengthPasswordValidator : IPasswordValidator<User>
{
    public const string ErrorCode = "PasswordTooLong";

    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string? password) =>
        Task.FromResult(password is { Length: > PasswordPolicy.MaxLength }
            ? IdentityResult.Failed(new IdentityError
            {
                Code = ErrorCode,
                Description = $"Passwords must be at most {PasswordPolicy.MaxLength} characters.",
            })
            : IdentityResult.Success);
}
