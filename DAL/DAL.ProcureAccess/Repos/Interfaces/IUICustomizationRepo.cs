namespace DAL.ProcureAccess.Repos.Interfaces;

public interface IUICustomizationRepo
{
    Task<UICustomization> GetAsync(string userId);
    Task UpdateAsync(string userId, UICustomizationDto dto);
}
