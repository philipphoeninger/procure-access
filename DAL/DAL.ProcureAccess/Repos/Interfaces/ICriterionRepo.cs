namespace DAL.ProcureAccess.Repos.Interfaces;

public interface ICriterionRepo : ITemporalTableBaseRepo<Criterion>, IApproveRepo<CriterionDto>
{
    IEnumerable<Criterion> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
