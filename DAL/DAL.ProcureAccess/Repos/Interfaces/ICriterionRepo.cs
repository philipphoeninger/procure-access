namespace DAL.ProcureAccess.Repos.Interfaces;

public interface ICriterionRepo : ITemporalTableBaseRepo<Criterion>
{
    IEnumerable<Criterion> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
