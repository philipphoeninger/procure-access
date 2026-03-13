namespace MODELS.ProcureAccess.Entities;

[Table("UICustomization", Schema = "dbo")]
[EntityTypeConfiguration(typeof(UICustomizationConfiguration))]
public class UICustomization : BaseEntity
{
    #region fields
    [StringLength(10)]
    public string ForegroundColor { get; set; }

    [StringLength(10)]
    public string BackgroundColor { get; set; }

    public bool DarkModeOn { get; set; }

    [StringLength(10)]
    public string TextColor { get; set; }

    public bool OrientationVertical { get; set; }

    public bool HighContrastOn { get; set; }

    public string UserId { get; set; }
    public virtual User User { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
    #endregion

        #region ctors
    public UICustomization()
    {
        CreatedAt = DateTime.UtcNow;
        ForegroundColor = "#111111";
        BackgroundColor = "#dbdedf";
        TextColor = "#111111";
        DarkModeOn = false;
        OrientationVertical = true;
        HighContrastOn = false;
    }

    public UICustomization(bool darkModeOn)
    {
        CreatedAt = DateTime.UtcNow;
        ForegroundColor = "#dbdedf";
        BackgroundColor = "#111111";
        TextColor = "#dbdedf";
        DarkModeOn = darkModeOn;
        OrientationVertical = true;
        HighContrastOn = false;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The UICustomization has the ID {Id}";
    }
    #endregion
}
