// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// Table <c>identity."Organizations"</c> (ARCHITECTURE §5.2). The organisation filter is bound in
/// <see cref="IdentityAccessDbContext"/>, because it needs the context's organisation set.
/// </summary>
internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public const string Table = "Organizations";

    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(Organization.NameMaxLength);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CreatedByUserId).IsRequired();
    }
}
