namespace DAL.ProcureAccess.Repos.Interfaces;

public interface IUICustomizationRepo
{
    Task<UICustomizationDto> GetAsync(string userId);
    Task UpdateAsync(string userId, UICustomizationDto dto);
}
