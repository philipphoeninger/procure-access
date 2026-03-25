namespace MODELS.ProcureAccess.Entities.Dto;

public class CriterionDto : BaseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? CriteriaFilterId { get; set; }
    public bool? IsDeleted { get; set; }
}
