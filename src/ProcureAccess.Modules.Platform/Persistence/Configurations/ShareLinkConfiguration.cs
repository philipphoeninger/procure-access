// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Platform.Sharing;

namespace ProcureAccess.Modules.Platform.Persistence.Configurations;

/// <summary>
/// Table <c>sharing."ShareLinks"</c> (ARCHITECTURE §5.3, C59; S5-23): the token hash is unique and
/// the lookup key of the anonymous read; the management lists a resource's links. No foreign key to
/// the organisation or the resource: they live in other module contexts (ARCHITECTURE §10.5). The
/// organisation filter is bound in <see cref="PlatformDbContext"/>.
/// </summary>
internal sealed class ShareLinkConfiguration : IEntityTypeConfiguration<ShareLink>
{
    public const string Schema = "sharing";
    public const string Table = "ShareLinks";

    public void Configure(EntityTypeBuilder<ShareLink> builder)
    {
        builder.ToTable(Table, Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(ShareLink.HashLength);
        builder.Property(x => x.ResourceType).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.ResourceType, x.ResourceId });
    }
}
