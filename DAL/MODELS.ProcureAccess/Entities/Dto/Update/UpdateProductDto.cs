namespace MODELS.ProcureAccess.Entities.Dto;

public class UpdateProductDto : BaseDto
{
    [StringLength(200)]
    public string? Name { get; set; }

    [Url]
    [StringLength(500)]
    public string? Link { get; set; }

    [StringLength(6000)]
    public string? Description { get; set; }
}
