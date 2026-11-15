// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// Table <c>identity."OrganizationInvitations"</c> (ARCHITECTURE §5.2, §9.1; S5-17a): the token hash
/// is unique and the lookup key of the link; at most one open invitation per organisation and
/// address (a new one replaces it, owner decision S5-17 D3 (a)); deleted with its organisation. The
/// organisation filter is bound in <see cref="IdentityAccessDbContext"/>.
/// </summary>
internal sealed class OrganizationInvitationConfiguration : IEntityTypeConfiguration<OrganizationInvitation>
{
    public const string Table = "OrganizationInvitations";

    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(OrganizationInvitation.EmailMaxLength);
        builder.Property(x => x.NormalizedEmail).IsRequired().HasMaxLength(OrganizationInvitation.EmailMaxLength);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(AccountToken.HashLength);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.NormalizedEmail })
               .IsUnique()
               .HasFilter("\"AcceptedAt\" IS NULL");
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasOne<Organization>()
               .WithMany()
               .HasForeignKey(x => x.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
