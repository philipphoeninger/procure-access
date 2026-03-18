namespace MODELS.ProcureAccess.Entities;

[EntityTypeConfiguration(typeof(UserConfiguration))]
public class User : IdentityUser
{
    #region fields
    [Required]
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; } = null;

    private UICustomization _uiCustomization = new();
    public UICustomization UICustomization
    {
        get => _uiCustomization;
        set => _uiCustomization = value ?? new UICustomization();
    }
    #endregion
}
