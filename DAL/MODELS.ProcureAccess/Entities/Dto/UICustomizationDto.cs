namespace MODELS.ProcureAccess.Entities.Dto;

public class UICustomizationDto : BaseDto
{
    public string? BackgroundColor { get; set; }
    public string? ForegroundColor { get; set; }
    public string? TextColor { get; set; }
    public bool? DarkModeOn { get; set; }
    public bool? HighContrastOn { get; set; }
    public bool? OrientationVertical { get; set; }
}
