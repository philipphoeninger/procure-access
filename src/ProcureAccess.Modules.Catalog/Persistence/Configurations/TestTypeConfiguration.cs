// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence.Seed;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class TestTypeConfiguration : IEntityTypeConfiguration<TestType>
{
    public void Configure(EntityTypeBuilder<TestType> builder)
    {
        builder.ConfigureFilterValue("TestTypes");
        builder.Property(x => x.Url).HasMaxLength(TestType.UrlMaxLength);
        builder.HasData(FilterValueSeed.TestTypes);
    }
}
