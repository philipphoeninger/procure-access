namespace MODELS.ProcureAccess.Entities;

[Table("Proposals", Schema = "dbo")]
[EntityTypeConfiguration(typeof(ProposalConfiguration))]
public partial class Proposal : BaseEntity
{
    #region fields
    [Required]
    public string ProposerId { get; set; }
    public User Proposer { get; set; }

    public string? ApproverId { get; set; }
    public User? Approver { get; set; }

    [ForeignKey("Product")]
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    [Column(TypeName = "nvarchar(max)")]
    public string? ProductSnapshot { get; set; }

    [ForeignKey("Criterion")]
    public int? CriterionId { get; set; }
    public Criterion? Criterion { get; set; }
    [Column(TypeName = "nvarchar(max)")]
    public string? CriterionSnapshot { get; set; }

    [Required]
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

    [Column(TypeName = "nvarchar(max)")]
    [StringLength(5000)]
    public string? Note { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;
    #endregion

    #region ctors
    public Proposal()
    {
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Proposal has the ID {Id}";
    }
    #endregion
}
