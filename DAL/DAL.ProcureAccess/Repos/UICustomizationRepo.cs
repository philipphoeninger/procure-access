namespace DAL.ProcureAccess.Repos;

public class UICustomizationRepo : TemporalTableBaseRepo<UICustomization>, IUICustomizationRepo
{
    #region ctors
    public UICustomizationRepo(ApplicationDBContext context) : base(context) { }
    internal UICustomizationRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<UICustomization> GetAll()
    // {
    //     return Context.UICustomization;
    // }
    public IEnumerable<UICustomization> GetAll(string userId) => Table.AsQueryable();

    #endregion
}
