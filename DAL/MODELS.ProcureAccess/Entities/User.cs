namespace MODELS.ProcureAccess.Entities;

[EntityTypeConfiguration(typeof(UserConfiguration))]
public class User : IdentityUser
{
    #region fields
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    public ICollection<Proposal> Approvals { get; set; } = new List<Proposal>();
    
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
