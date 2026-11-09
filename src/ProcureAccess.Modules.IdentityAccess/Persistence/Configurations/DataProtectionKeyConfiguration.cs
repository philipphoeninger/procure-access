// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// Table <c>identity."DataProtectionKeys"</c> (ARCHITECTURE §9.1). The entity belongs to ASP.NET Core
/// Data Protection; its shape stays as the library defines it. The keys are stored unencrypted
/// (owner decision S3-24 3a): whoever can read the database or its backup can read them.
/// </summary>
internal sealed class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public const string Table = "DataProtectionKeys";

    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
    }
}
