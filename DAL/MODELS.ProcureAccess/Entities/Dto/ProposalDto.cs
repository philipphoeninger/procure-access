namespace MODELS.ProcureAccess.Entities.Dto;

public class ProposalDto : BaseDto
{
    public string ProposerId { get; set; }
    public string? ApproverId { get; set; }
    public int? ProductId { get; set; }
    public int? CriterionId { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovalNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public bool IsDeleted { get; set; }
}
