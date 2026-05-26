namespace MODELS.ProcureAccess.Entities.Base;

public abstract class BaseEntity
{
    public int Id { get; set; }
    
    [Required]
    public DateTimeOffset CreatedAt { get; set; }
    
    public uint xmin { get; set; }
}
