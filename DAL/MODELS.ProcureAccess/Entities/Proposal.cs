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

    [ForeignKey("Criterion")]
    public int? CriterionId { get; set; }
    public Criterion? Criterion { get; set; }

    [Required]
    public bool IsApproved { get; set; } = false;

    [Column(TypeName = "nvarchar(max)")]
    [StringLength(5000)]
    public string? ApprovalNote { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; }
    #endregion

    #region ctors
    public Proposal()
    {
        CreatedAt = DateTime.UtcNow;
        IsApproved = false;
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
