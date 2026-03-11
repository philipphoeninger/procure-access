namespace DAL.ProcureAccess.Repos.Interfaces;

public interface ICriterionRepo : ITemporalTableBaseRepo<Criterion>
{
    public IEnumerable<Criterion> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
