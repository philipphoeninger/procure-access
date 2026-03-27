namespace SERVICES.ProcureAccess.DataServices;

public class CriterionService : BaseService<Criterion, CriterionDto>, ICriterionService
{
    public CriterionService(ICriterionRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public IEnumerable<Criterion> GetByCriteriaFilterIds(int[] criteriaFilterIds)
    {
        return MainRepo.Context.Criteria.Where(x => criteriaFilterIds.Any(a => a == x.CriteriaFilterId)).ToList();
    }

    public int Approve(CriterionDto dto)
    {
        var criteria = MainRepo.FindIgnoreQueryFilters(dto.Id);

        if (dto.Name != null)
            criteria.Name = dto.Name;
        if (dto.Description != null)
            criteria.Description = dto.Description;
        if (dto.CriteriaFilterId != null)
            criteria.CriteriaFilterId = dto.CriteriaFilterId;

        if (dto.IsDeleted.HasValue)
            criteria.IsDeleted = dto.IsDeleted.Value;

        // approve
        criteria.ToApprove = false;

        return MainRepo.SaveChanges();
    }
}
