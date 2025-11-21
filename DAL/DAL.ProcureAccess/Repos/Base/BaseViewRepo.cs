namespace DAL.ProcureAccess.Repos.Base;

public abstract class BaseViewRepo<T> : IBaseViewRepo<T> where T : class, new()
{
    #region fields
    private readonly bool _disposeContext;
    public ApplicationDBContext Context { get; }
    public DbSet<T> Table { get; }
    #endregion

    #region ctors
    protected BaseViewRepo(ApplicationDBContext context)
    {
        Context = context;
        Table = Context.Set<T>();
        _disposeContext = false;
    }

    protected BaseViewRepo(DbContextOptions<ApplicationDBContext> options)
        : this(new ApplicationDBContext(options))
    {
        _disposeContext = true;
    }
    #endregion

    #region methods
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

    public virtual IEnumerable<T> GetAll(string userId) => Table.AsQueryable();

    public virtual IEnumerable<T> GetAllIgnoreQueryFilters() => Table.AsQueryable().IgnoreQueryFilters();

    public IEnumerable<T> ExecuteSqlString(string sql) => Table.FromSqlRaw(sql);
    #endregion

    ~BaseViewRepo() // finalizer
    {
        Dispose(false);
    }
}
