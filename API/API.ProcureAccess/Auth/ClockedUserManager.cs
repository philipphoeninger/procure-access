// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Auth;

/// <summary>
/// Identity's <see cref="UserManager{TUser}"/> with the lockout on the API's
/// <see cref="TimeProvider"/> (CLAUDE §7; S5-13a): Identity 10 reads the system clock for the lock's
/// start and end, so <c>FakeTimeProvider</c> could not drive the lockout of SEC-11. Only the two
/// methods that read the clock are replaced; they follow Identity's own implementation. Creating a
/// user also stamps <see cref="User.CreatedAt"/> from the same clock (SPEC §8.4; S5-27).
/// </summary>
/// <remarks>
/// <see cref="SignInManager{TUser}"/> asks <see cref="IsLockedOutAsync"/> before it checks the
/// password and calls <see cref="AccessFailedAsync"/> after a wrong one, so both paths of the
/// sign-in use this clock. It moves into IdentityAccess with the users (M3.1).
/// </remarks>
public sealed class ClockedUserManager(
    IUserStore<User> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<User> passwordHasher,
    IEnumerable<IUserValidator<User>> userValidators,
    IEnumerable<IPasswordValidator<User>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<User>> logger,
    TimeProvider clock)
    : UserManager<User>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
{
    /// <summary>Stores <paramref name="user"/> with <see cref="User.CreatedAt"/> = now, unless the caller set it.</summary>
    public override Task<IdentityResult> CreateAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.CreatedAt ??= clock.GetUtcNow();
        return base.CreateAsync(user);
    }

    /// <summary>Locked while the lockout end lies at or after now, as Identity decides it.</summary>
    public override async Task<bool> IsLockedOutAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!await GetLockoutEnabledAsync(user))
        {
            return false;
        }

        var lockoutEnd = await GetLockoutEndDateAsync(user);
        return lockoutEnd >= clock.GetUtcNow();
    }

    /// <summary>
    /// Counts a failed sign-in; at the 🔧 limit locks the account for the 🔧 duration from now and
    /// resets the count.
    /// </summary>
    public override async Task<IdentityResult> AccessFailedAsync(User user)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var lockoutStore = Store as IUserLockoutStore<User>
            ?? throw new NotSupportedException("The user store does not support lockout.");

        var count = await lockoutStore.IncrementAccessFailedCountAsync(user, CancellationToken);
        if (count >= Options.Lockout.MaxFailedAccessAttempts)
        {
            await lockoutStore.SetLockoutEndDateAsync(user, clock.GetUtcNow().Add(Options.Lockout.DefaultLockoutTimeSpan), CancellationToken);
            await lockoutStore.ResetAccessFailedCountAsync(user, CancellationToken);
        }

        return await UpdateUserAsync(user);
    }
}
