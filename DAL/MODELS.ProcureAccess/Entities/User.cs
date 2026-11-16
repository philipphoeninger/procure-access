// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities;

public class User : IdentityUser
{
    #region fields
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    public ICollection<Proposal> Approvals { get; set; } = new List<Proposal>();

    [Required]
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; } = null;

    /// <summary>
    /// When the account was created, set by the API's user manager from its <c>TimeProvider</c>;
    /// <c>null</c> for accounts created before S5-27. The personal data export shows it
    /// (SPEC §8.4 <c>account.createdAt</c>; owner decision S5-27 2a).
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    private UICustomization _uiCustomization = new();
    public UICustomization UICustomization
    {
        get => _uiCustomization;
        set => _uiCustomization = value ?? new UICustomization();
    }
    #endregion
}
