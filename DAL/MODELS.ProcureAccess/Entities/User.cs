namespace MODELS.ProcureAccess.Entities;

public class User : IdentityUser
{
    #region fields
    public UICustomization? UICustomization { get; set; }
    #endregion
}
