namespace MODELS.ProcureAccess.Entities;

public class UICustomization
{
    #region fields
    public string ForegroundColor { get; set; } = "#111111";

    public string BackgroundColor { get; set; } = "#dbdedf";

    public string TextColor { get; set; } = "#111111";

    public bool DarkModeOn { get; set; } = false;

    public bool OrientationVertical { get; set; } = true;

    public bool HighContrastOn { get; set; } = false;
    #endregion

        #region ctors
    public UICustomization() {}
    #endregion
}
