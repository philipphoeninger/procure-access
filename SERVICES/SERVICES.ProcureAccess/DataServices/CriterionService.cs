namespace SERVICES.ProcureAccess.DataServices;

public class CriterionService : BaseService<Criterion, CriterionDto>, ICriterionService
{
    public CriterionService(ICriterionRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override async Task<Result<IEnumerable<CriterionDto>>> ReadAll()
    {
        List<Criterion> entities = MainRepo.Context.Criteria.Where(x => 
            x.Proposal == null || x.Proposal.Status == ProposalStatus.Approved).ToList();
        List<CriterionDto> dtos = new List<CriterionDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<CriterionDto>(x)));
        return Result<IEnumerable<CriterionDto>>.Success(dtos);
    }

    public IEnumerable<CriterionDto> GetByCriteriaFilterIds(int[] criteriaFilterIds)
    {
        List<Criterion> criteria = 
            MainRepo.Context.Criteria.Where(x => 
                criteriaFilterIds.Any(a => a == x.CriteriaFilterId)).ToList();
        List<CriterionDto> criteriaDtos = new List<CriterionDto>();
        criteria.ForEach(x => criteriaDtos.Add(Mapper.Map<CriterionDto>(x)));
        return criteriaDtos;
    }
}
