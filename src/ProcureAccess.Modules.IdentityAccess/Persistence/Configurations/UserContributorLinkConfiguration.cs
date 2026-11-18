// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// Table <c>identity."UserContributorLinks"</c> (ARCHITECTURE §5.2): one row per profile, deleted
/// with its profile. <c>UserId</c> is a foreign key onto the users since S6-10b (cascade).
/// </summary>
internal sealed class UserContributorLinkConfiguration : IEntityTypeConfiguration<UserContributorLink>
{
    public const string Table = "UserContributorLinks";

    public void Configure(EntityTypeBuilder<UserContributorLink> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.ContributorProfileId);
        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ContributorProfile>()
               .WithOne()
               .HasForeignKey<UserContributorLink>(x => x.ContributorProfileId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
