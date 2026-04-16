namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface ICriterionService : IBaseService<Criterion, CriterionDto>
{
    IEnumerable<CriterionDto> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
