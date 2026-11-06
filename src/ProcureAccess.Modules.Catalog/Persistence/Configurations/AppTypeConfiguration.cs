// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence.Seed;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class AppTypeConfiguration : IEntityTypeConfiguration<AppType>
{
    public void Configure(EntityTypeBuilder<AppType> builder)
    {
        builder.ConfigureFilterValue("AppTypes");
        builder.Property(x => x.HintDe).HasMaxLength(AppType.HintMaxLength);
        builder.Property(x => x.Functionality).HasConversion<string>().HasMaxLength(32);
        builder.HasData(FilterValueSeed.AppTypes);
    }
}
