namespace MODELS.ProcureAccess.Entities.Requests;

public class UpsertProposalCommand
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }
    public CreateProductDto? Product { get; set; }

    public int? CriterionId { get; set; }
    public CreateCriterionDto? Criterion { get; set; }
}
