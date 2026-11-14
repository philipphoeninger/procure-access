// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// Table <c>identity."AccountTokens"</c> (ARCHITECTURE §9.1, C72; S5-12a): the hash is unique and
/// the lookup key; <c>UserId</c> has no foreign key, because the users are still in the legacy
/// context (ADR-014).
/// </summary>
internal sealed class AccountTokenConfiguration : IEntityTypeConfiguration<AccountToken>
{
    public const string Table = "AccountTokens";

    public void Configure(EntityTypeBuilder<AccountToken> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(AccountToken.HashLength);
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
