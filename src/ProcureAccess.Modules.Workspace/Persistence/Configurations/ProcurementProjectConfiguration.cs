// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Workspace.Domain;

namespace ProcureAccess.Modules.Workspace.Persistence.Configurations;

/// <summary>
/// Table <c>workspace."ProcurementProjects"</c> (ARCHITECTURE §5.4). <c>OrganizationId</c> and
/// <c>CreatedByUserId</c> have no foreign key, because organisations and users live in another
/// context (ADR-014). The organisation filter is bound in <see cref="WorkspaceDbContext"/>,
/// because it needs the context's organisation set.
/// </summary>
internal sealed class ProcurementProjectConfiguration : IEntityTypeConfiguration<ProcurementProject>
{
    public const string Table = "ProcurementProjects";

    public void Configure(EntityTypeBuilder<ProcurementProject> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(ProcurementProject.NameMaxLength);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CreatedByUserId).IsRequired();
        builder.Ignore(x => x.IsArchived);

        // The concurrency token behind the ETag (API-03, S6-20); rows older than it start at 1.
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(1);

        // The projects of an organisation (S30) and the projects a user created (personal data export, S6-25).
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.CreatedByUserId);
    }
}
