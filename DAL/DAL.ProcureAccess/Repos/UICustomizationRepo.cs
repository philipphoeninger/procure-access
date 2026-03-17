namespace DAL.ProcureAccess.Repos;

public class UICustomizationRepo : IUICustomizationRepo
{
    private readonly bool _disposeContext;
    public ApplicationDBContext Context { get; }

    #region ctors
    public UICustomizationRepo(ApplicationDBContext context)
    {
        Context = context;
    }
    protected UICustomizationRepo(DbContextOptions<ApplicationDBContext> options)
        : this(new ApplicationDBContext(options))
    {
        _disposeContext = true;
    }
    #endregion

    #region methods
    public async Task<UICustomization> GetAsync(string userId)
    {
        return await Context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.UICustomization)
            .FirstAsync();
    }

    public async Task UpdateAsync(string userId, UpdateUICustomizationDto dto)
    {
        var user = await Context.Users.FirstAsync(u => u.Id == userId);

        if (dto.BackgroundColor != null)
            user.UICustomization.BackgroundColor = dto.BackgroundColor;
        if (dto.ForegroundColor != null)
            user.UICustomization.ForegroundColor = dto.ForegroundColor;
        if (dto.TextColor != null)
            user.UICustomization.TextColor = dto.TextColor;

        if (dto.DarkModeOn.HasValue)
            user.UICustomization.DarkModeOn = dto.DarkModeOn.Value;
        if (dto.HighContrastOn.HasValue)
            user.UICustomization.HighContrastOn = dto.HighContrastOn.Value;
        if (dto.OrientationVertical.HasValue)
            user.UICustomization.OrientationVertical = dto.OrientationVertical.Value;

        await Context.SaveChangesAsync();
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private bool _isDisposed;
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }
        if (disposing)
        {
            if (_disposeContext)
            {
                Context.Dispose();
            }
        }
        _isDisposed = true;
    }
    #endregion

    ~UICustomizationRepo() // finalizer
    {
        Dispose(false);
    }
}
