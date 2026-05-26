namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProposalConfiguration : BaseEntityConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
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

        builder
            .HasOne(x => x.Proposer)
            .WithMany(u => u.Proposals)
            .HasForeignKey(x => x.ProposerId)
            .IsRequired(true);
        builder
            .HasOne(x => x.Approver)
            .WithMany(u => u.Approvals)
            .HasForeignKey(x => x.ApproverId)
            .IsRequired(false);
        
        // Check Constraints
        builder.ToTable(b => b.HasCheckConstraint(
            "CK_Proposal_Snapshot_NN",
            @"(""ProductSnapshot"" IS NULL) <> (""CriterionSnapshot"" IS NULL)"));
    }
}
