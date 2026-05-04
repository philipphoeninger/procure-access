namespace SERVICES.ProcureAccess.DataServices;

public class FilterTypeService : BaseService<FilterType, FilterTypeDto>, IFilterTypeService
{
    public FilterTypeService(IFilterTypeRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }
}
