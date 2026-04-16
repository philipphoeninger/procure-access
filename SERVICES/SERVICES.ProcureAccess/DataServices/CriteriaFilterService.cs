namespace SERVICES.ProcureAccess.DataServices;

public class CriteriaFilterService : BaseService<CriteriaFilter, CriteriaFilterDto>, ICriteriaFilterService
{
    public CriteriaFilterService(ICriteriaFilterRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }
}
