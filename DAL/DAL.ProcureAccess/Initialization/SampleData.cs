// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Initialization;

/// <summary>
/// Sample users for the local Development stack. The repository is public: no real addresses
/// and no password hashes here. The password comes from configuration (see
/// <see cref="SampleDataInitializer"/>).
/// </summary>
public static class SampleData
{
    public const string SampleUserEmail = "sample.user@example.org";

    public static List<User> Users => new()
    {
        new()
        {
            UserName = SampleUserEmail,
            Email = SampleUserEmail,
            UICustomization = new(),
            EmailConfirmed = true
        }
    };
}
