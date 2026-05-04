namespace MODELS.ProcureAccess.Entities.Dto;

public class UpdateCriterionDto : BaseDto
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(6000)]
    public string? Description { get; set; }
    
    public int? CriteriaFilterId { get; set; }
}
