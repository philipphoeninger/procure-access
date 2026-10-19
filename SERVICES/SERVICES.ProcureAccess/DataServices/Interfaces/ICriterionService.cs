namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface ICriterionService : IBaseService<Criterion, CriterionDto>
{
    Task<IEnumerable<CriterionDto>> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
