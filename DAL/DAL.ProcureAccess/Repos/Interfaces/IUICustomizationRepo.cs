namespace DAL.ProcureAccess.Repos.Interfaces;

public interface IUICustomizationRepo : ITemporalTableBaseRepo<UICustomization>
{
    IEnumerable<UICustomization> GetAll(string userId);
}
