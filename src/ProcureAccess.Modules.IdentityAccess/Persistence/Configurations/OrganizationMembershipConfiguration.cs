// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// Table <c>identity."OrganizationMemberships"</c> (ARCHITECTURE §5.2): one row per organisation
/// and user, deleted with its organisation. <c>UserId</c> is a foreign key onto the users since
/// S6-10b (cascade); its index serves the per-request loader.
/// </summary>
internal sealed class OrganizationMembershipConfiguration : IEntityTypeConfiguration<OrganizationMembership>
{
    public const string Table = "OrganizationMemberships";

    public void Configure(EntityTypeBuilder<OrganizationMembership> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => new { x.OrganizationId, x.UserId });
        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(16);
        builder.HasOne<Organization>()
               .WithMany()
               .HasForeignKey(x => x.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
