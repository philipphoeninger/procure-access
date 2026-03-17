namespace MODELS.ProcureAccess.Entities;

[EntityTypeConfiguration(typeof(UserConfiguration))]
public class User : IdentityUser
{
    #region fields
    private UICustomization _uiCustomization = new();
    public UICustomization UICustomization
    {
        get => _uiCustomization;
        set => _uiCustomization = value ?? new UICustomization();
    }
    #endregion
}
