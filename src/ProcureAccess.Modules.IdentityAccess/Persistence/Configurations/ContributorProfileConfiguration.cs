// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>Table <c>identity."ContributorProfiles"</c> (ARCHITECTURE §5.2).</summary>
internal sealed class ContributorProfileConfiguration : IEntityTypeConfiguration<ContributorProfile>
{
    public const string Table = "ContributorProfiles";

    public void Configure(EntityTypeBuilder<ContributorProfile> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Unique regardless of case (SPEC §7.2): the unique index compares with the
        // case-insensitive collation of IdentityAccessDbContext.
        builder.Property(x => x.DisplayName)
               .HasMaxLength(ContributorProfile.DisplayNameMaxLength)
               .UseCollation(IdentityAccessDbContext.CaseInsensitiveCollation);
        builder.HasIndex(x => x.DisplayName).IsUnique();

        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
    }
}
