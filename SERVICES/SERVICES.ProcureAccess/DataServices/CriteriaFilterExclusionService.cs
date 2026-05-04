namespace SERVICES.ProcureAccess.DataServices;

public class CriteriaFilterExclusionService : BaseService<CriteriaFilterExclusion, CriteriaFilterExclusionDto>, ICriteriaFilterExclusionService
{
    public CriteriaFilterExclusionService(ICriteriaFilterExclusionRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }
}
