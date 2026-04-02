namespace MODELS.ProcureAccess.Entities.Dto;

public class CriteriaFilterDto : BaseDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? FilterTypeId { get; set; }
    public bool? IsDeleted { get; set; }
}
