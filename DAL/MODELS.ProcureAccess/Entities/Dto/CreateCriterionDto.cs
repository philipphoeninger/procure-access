namespace MODELS.ProcureAccess.Entities.Dto;

public class CreateCriterionDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [StringLength(6000)]
    public string Description { get; set; }

    public int CriteriaFilterId { get; set; }
}
