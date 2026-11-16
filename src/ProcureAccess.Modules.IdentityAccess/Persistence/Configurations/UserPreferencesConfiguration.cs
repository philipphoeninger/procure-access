// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// One row per user (S5-25a). <c>UserId</c> has no foreign key, because the users are still the
/// legacy context's (ADR-014); not organisation-scoped, so no query filter: every read names the
/// signed-in user's id.
/// </summary>
internal sealed class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public const string Table = "UserPreferences";

    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.UserId).ValueGeneratedNever();
        builder.Property(x => x.Language).HasMaxLength(UserPreferences.LanguageMaxLength);
        builder.Property(x => x.Appearance).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Motion).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Contrast).HasConversion<string>().HasMaxLength(16);
    }
}
