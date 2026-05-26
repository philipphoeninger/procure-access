namespace MODELS.ProcureAccess.Entities.Configuration;

public class CriteriaFilterExclusionConfiguration
    : BaseEntityConfiguration<CriteriaFilterExclusion>
{
    public void Configure(EntityTypeBuilder<CriteriaFilterExclusion> builder)
    {
        base.Configure(builder);
        
        // Query Filters
        builder.HasQueryFilter(x => 
            !x.CriteriaFilter.FilterType.IsDeleted && !x.Exclusion.FilterType.IsDeleted);

        // Indices
        builder.HasIndex(
            x => new { x.CriteriaFilterId, x.ExclusionId }).IsUnique();

        // Properties
        builder
            .HasOne(x => x.CriteriaFilter)
            .WithMany(cf => cf.Exclusions)
            .HasForeignKey(x => x.CriteriaFilterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Exclusion)
            .WithMany(cf => cf.ParentExclusions)
            .HasForeignKey(x => x.ExclusionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
