// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Configurator.Domain;

namespace ProcureAccess.Modules.Configurator.Persistence.Configurations;

/// <summary>
/// Table <c>configurator."CriteriaConfigurations"</c> and the four tables of its rows
/// (ARCHITECTURE §5.3). The rows are owned collections: deleted with their list, loaded with it and
/// without a set of their own. <c>OrganizationId</c> and <c>CreatedByUserId</c> have no foreign
/// key, because organisations and users live in other contexts (ADR-014); the release and the
/// values are named by version and key (owner decision S5-19 2a). The organisation filter is bound
/// in <see cref="ConfiguratorDbContext"/>, because it needs the context's organisation set.
/// </summary>
internal sealed class CriteriaConfigurationConfiguration : IEntityTypeConfiguration<CriteriaConfiguration>
{
    public const string Table = "CriteriaConfigurations";
    public const string AppTypesTable = "ConfigurationAppTypes";
    public const string ProductPartsTable = "ConfigurationProductParts";
    public const string TestTypesTable = "ConfigurationTestTypes";
    public const string CriteriaTable = "ConfigurationCriteria";

    /// <summary>The column of every row table that names its list.</summary>
    public const string OwnerKey = "ConfigurationId";

    public void Configure(EntityTypeBuilder<CriteriaConfiguration> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(CriteriaConfiguration.NameMaxLength);
        builder.Property(x => x.ReleaseVersion).HasMaxLength(CriteriaConfiguration.ReleaseVersionMaxLength);
        builder.Property(x => x.UseCaseKey).HasMaxLength(CriteriaConfiguration.ValueKeyMaxLength);
        builder.Property(x => x.CreatedByUserId).IsRequired();

        // The lists of an organisation (S14) and the lists a user created (personal data export).
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.CreatedByUserId);

        builder.OwnsMany(x => x.AppTypes, rows =>
        {
            rows.ToTable(AppTypesTable);
            rows.WithOwner().HasForeignKey(OwnerKey);
            rows.Property(x => x.AppTypeKey).HasMaxLength(CriteriaConfiguration.ValueKeyMaxLength);
            rows.HasKey(OwnerKey, nameof(ConfigurationAppType.AppTypeKey));
        });
        builder.OwnsMany(x => x.ProductParts, rows =>
        {
            rows.ToTable(ProductPartsTable);
            rows.WithOwner().HasForeignKey(OwnerKey);
            rows.Property(x => x.ProductPartKey).HasMaxLength(CriteriaConfiguration.ValueKeyMaxLength);
            rows.HasKey(OwnerKey, nameof(ConfigurationProductPart.ProductPartKey));
        });
        builder.OwnsMany(x => x.TestTypes, rows =>
        {
            rows.ToTable(TestTypesTable);
            rows.WithOwner().HasForeignKey(OwnerKey);
            rows.Property(x => x.TestTypeKey).HasMaxLength(CriteriaConfiguration.ValueKeyMaxLength);
            rows.HasKey(OwnerKey, nameof(ConfigurationTestType.TestTypeKey));
        });
        builder.OwnsMany(x => x.Criteria, rows =>
        {
            rows.ToTable(CriteriaTable);
            rows.WithOwner().HasForeignKey(OwnerKey);
            rows.HasKey(OwnerKey, nameof(ConfigurationCriterion.CriterionRevisionId));
            rows.Property(x => x.CriterionRevisionId).ValueGeneratedNever();
            rows.Property(x => x.ResolvedLevel).HasConversion<string>().HasMaxLength(16);
            rows.Property(x => x.OverrideLevel).HasConversion<string>().HasMaxLength(16);
            rows.Property(x => x.NoteDe).HasMaxLength(ConfigurationCriterion.NoteMaxLength);
            rows.Ignore(x => x.EffectiveLevel);
            rows.Ignore(x => x.HasDecision);
        });
    }
}
