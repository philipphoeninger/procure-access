// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence.Seed;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class UseCaseTemplateConfiguration : IEntityTypeConfiguration<UseCaseTemplate>
{
    public void Configure(EntityTypeBuilder<UseCaseTemplate> builder)
    {
        builder.ConfigureFilterValue("UseCaseTemplates");
        builder.Property(x => x.Description).HasMaxLength(UseCaseTemplate.DescriptionMaxLength);
        builder.HasData(FilterValueSeed.UseCases);
    }
}
