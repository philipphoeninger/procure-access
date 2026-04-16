namespace MODELS.ProcureAccess.Entities.Requests;

public class ReviewProposalCommand
{
    public int ProposalId { get; set; }
    public CreateProductDto? Product { get; set; }
    public CreateCriterionDto? Criterion { get; set; }
}
