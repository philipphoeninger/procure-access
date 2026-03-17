namespace MODELS.ProcureAccess.Entities.Dto;

public class UpdateUICustomizationDto
{
    public string? BackgroundColor { get; set; }
    public string? ForegroundColor { get; set; }
    public string? TextColor { get; set; }
    public bool? DarkModeOn { get; set; }
    public bool? HighContrastOn { get; set; }
    public bool? OrientationVertical { get; set; }
}
