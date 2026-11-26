// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Workspace.Domain;

namespace ProcureAccess.Modules.Workspace.Persistence.Configurations;

/// <summary>
/// Table <c>workspace."CandidateProducts"</c> (ARCHITECTURE §5.4; S6-15). <c>TestStatus</c> is
/// stored by name, not null, with the database default <c>Unknown</c> (SIG-03 workspace part).
/// <c>SoftwareProductId</c> has no foreign key: the directory is another context (ADR-014).
/// </summary>
internal sealed class CandidateProductConfiguration : IEntityTypeConfiguration<CandidateProduct>
{
    public const string Table = "CandidateProducts";

    public void Configure(EntityTypeBuilder<CandidateProduct> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(CandidateProduct.NameMaxLength);
        builder.Property(x => x.VendorName).HasMaxLength(CandidateProduct.NameMaxLength);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(CandidateProduct.WebsiteUrlMaxLength);
        builder.Property(x => x.FunctionalSummary).HasMaxLength(CandidateProduct.FunctionalSummaryMaxLength);

        // The sentinel is a value the enum never has, so EF always sends the status and the
        // database default only fills rows written without it (SIG-03: never null, „Nicht bekannt“).
        builder.Property(x => x.TestStatus)
               .HasConversion<string>()
               .HasMaxLength(16)
               .HasDefaultValue(TestStatus.Unknown)
               .HasSentinel((TestStatus)(-1));

        // The concurrency token behind the ETag (API-03, S6-20); rows older than it start at 1.
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(1);

        builder.HasOne<ProcurementProject>()
               .WithMany()
               .HasForeignKey(x => x.ProcurementProjectId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TestEvidences)
               .WithOne()
               .HasForeignKey(x => x.CandidateProductId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.TestEvidences).UsePropertyAccessMode(PropertyAccessMode.Field);

        // The candidates of a project (S32, the product count); the foreign key's index.
        builder.HasIndex(x => x.ProcurementProjectId);
    }
}

/// <summary>
/// Table <c>workspace."CandidatePrivateSections"</c>: the private section of a candidate, keyed by
/// the candidate's id and deleted with it (S6-15). Neither type has a navigation to the other.
/// </summary>
internal sealed class CandidatePrivateSectionConfiguration : IEntityTypeConfiguration<CandidatePrivateSection>
{
    public const string Table = "CandidatePrivateSections";

    public void Configure(EntityTypeBuilder<CandidatePrivateSection> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.CandidateProductId);
        builder.Property(x => x.ContactNotes).HasMaxLength(CandidatePrivateSection.NotesMaxLength);
        builder.Property(x => x.InternalNotes).HasMaxLength(CandidatePrivateSection.NotesMaxLength);
        builder.HasOne<CandidateProduct>()
               .WithOne()
               .HasForeignKey<CandidatePrivateSection>(x => x.CandidateProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Table <c>workspace."TestEvidences"</c> (ARCHITECTURE §5.4, C46): at most one row per candidate
/// and catalog procedure — a unique index over the procedure rows (WSP-01).
/// </summary>
internal sealed class TestEvidenceConfiguration : IEntityTypeConfiguration<TestEvidence>
{
    public const string Table = "TestEvidences";

    /// <summary>
    /// The unique index over the procedure rows, by EF's default name; the evidence service tells
    /// its violation by this name (#453).
    /// </summary>
    public const string ProcedureIndex = "IX_TestEvidences_CandidateProductId_TestTypeKey";

    public void Configure(EntityTypeBuilder<TestEvidence> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TestKind).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.TestTypeKey).HasMaxLength(64);
        builder.Property(x => x.OtherTestTypeName).HasMaxLength(TestEvidence.OtherTestTypeNameMaxLength);
        builder.Property(x => x.TestedByName).HasMaxLength(TestEvidence.TestedByNameMaxLength);
        builder.Property(x => x.TestedVersion).HasMaxLength(TestEvidence.TestedVersionMaxLength);
        builder.Property(x => x.EvidenceUrl).HasMaxLength(TestEvidence.EvidenceUrlMaxLength);

        // The concurrency token behind the ETag (API-03, S6-20); rows older than it start at 1.
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(1);

        // Also the foreign key's index; the filter leaves „Andere“ and „Eigene Prüfung“ unlimited.
        builder.HasIndex(x => new { x.CandidateProductId, x.TestTypeKey })
               .IsUnique()
               .HasFilter("\"TestTypeKey\" IS NOT NULL")
               .HasDatabaseName(ProcedureIndex);
    }
}

/// <summary>
/// Table <c>workspace."TestEvidencePrivateNotes"</c>: the private note of an evidence, keyed by the
/// evidence's id and deleted with it (owner decision S6-15 4 (a)).
/// </summary>
internal sealed class TestEvidencePrivateNoteConfiguration : IEntityTypeConfiguration<TestEvidencePrivateNote>
{
    public const string Table = "TestEvidencePrivateNotes";

    public void Configure(EntityTypeBuilder<TestEvidencePrivateNote> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.TestEvidenceId);
        builder.Property(x => x.QualificationNote).HasMaxLength(TestEvidencePrivateNote.QualificationNoteMaxLength);
        builder.HasOne<TestEvidence>()
               .WithOne()
               .HasForeignKey<TestEvidencePrivateNote>(x => x.TestEvidenceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
