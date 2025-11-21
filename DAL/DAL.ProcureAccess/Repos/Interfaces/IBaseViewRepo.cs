namespace DAL.ProcureAccess.Repos.Interfaces;

public interface IBaseViewRepo<T> : IDisposable where T : class, new()
{
    ApplicationDBContext Context { get; }
    IEnumerable<T> ExecuteSqlString(string sql);
    IEnumerable<T> GetAll(string userId);
    IEnumerable<T> GetAllIgnoreQueryFilters();
}
