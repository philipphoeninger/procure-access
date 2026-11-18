// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProposalConfiguration : BaseEntityConfiguration<Proposal>
{
    public override void Configure(EntityTypeBuilder<Proposal> builder)
    {
        base.Configure(builder);

        // Query Filters
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Indices
        builder.HasIndex(
            x => new { x.ProductId, x.CriterionId }).IsUnique();

        // Properties
        builder.Property(x => x.Note).HasColumnType("text");

        builder
            .HasOne(x => x.Product)
            .WithOne(p => p.Proposal)
            .IsRequired(false);
        builder
            .HasOne(x => x.Criterion)
            .WithOne(p => p.Proposal)
            .IsRequired(false);

        // The proposer and the reviewer are user ids without a relation since S6-10 (ADR-014
        // rule 5); their indexes stay as the former foreign keys created them.
        builder.HasIndex(x => x.ProposerId);
        builder.HasIndex(x => x.ApproverId);

        // Check Constraints
        builder.ToTable(b => b.HasCheckConstraint(
            "CK_Proposal_Snapshot_NN",
            @"(""ProductSnapshot"" IS NULL) <> (""CriterionSnapshot"" IS NULL)"));
    }
}
