// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Initialization;

public static class SampleDataInitializer
{
    #region public
    /// <summary>
    /// Seeds the sample users with <paramref name="sampleUserPassword"/>. Without a password no
    /// user is created: a fixed password or hash would be public in the repository.
    /// </summary>
    /// <returns><c>true</c> if the sample users were created.</returns>
    public static async Task<bool> InitializeData(
        ApplicationDBContext dbContext, UserManager<User> userManager, string? sampleUserPassword)
    {
        return await SeedData(dbContext, userManager, sampleUserPassword);
    }

    public static async Task ClearDatabase(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        await ClearData(dbContext, userManager);
    }
    #endregion

    internal static async Task ClearData(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        foreach (User user in userManager.Users)
        {
            await userManager.DeleteAsync(user);
        }
    }

    internal static async Task<bool> SeedData(
        ApplicationDBContext dbContext, UserManager<User> userManager, string? sampleUserPassword)
    {
        if (string.IsNullOrEmpty(sampleUserPassword))
        {
            return false;
        }

        foreach (User user in SampleData.Users)
        {
            var result = await userManager.CreateAsync(user, sampleUserPassword);
            if (!result.Succeeded)
            {
                // Error codes only: the descriptions may repeat the password rules, never the password.
                throw new InvalidOperationException(
                    "Creating the sample user failed: " +
                    string.Join(", ", result.Errors.Select(e => e.Code)) +
                    ". The sample user password must meet the Identity password rules.");
            }
            await userManager.AddToRoleAsync(user, Roles.Member);
        }
        return true;
    }
}
