// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Workspace.Domain;

namespace ProcureAccess.Modules.Workspace.Persistence.Configurations;

/// <summary>
/// Table <c>workspace."CandidateAssessments"</c> (ARCHITECTURE §5.4, C53; S6-18): one row per
/// candidate and criterion revision — the key —, deleted with the candidate. The value is stored by
/// name. The revision is a catalog id without a foreign key (ADR-014).
/// </summary>
internal sealed class CandidateAssessmentConfiguration : IEntityTypeConfiguration<CandidateAssessment>
{
    public const string Table = "CandidateAssessments";

    public void Configure(EntityTypeBuilder<CandidateAssessment> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => new { x.CandidateProductId, x.CriterionRevisionId });
        builder.Property(x => x.CriterionRevisionId).ValueGeneratedNever();
        builder.Property(x => x.Fulfillment).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.EvidenceNote).HasMaxLength(CandidateAssessment.EvidenceNoteMaxLength);
        builder.HasOne<CandidateProduct>()
               .WithMany()
               .HasForeignKey(x => x.CandidateProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
