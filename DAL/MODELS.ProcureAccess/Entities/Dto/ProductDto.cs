namespace MODELS.ProcureAccess.Entities.Dto;

public class ProductDto : BaseDto
{
    public string? Name { get; set; }
    public string? Link { get; set; }
    public string? Description { get; set; }
    public int? TypeId { get; set; }
    public bool? IsDeleted { get; set; }
}
