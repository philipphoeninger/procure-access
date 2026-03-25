namespace DAL.ProcureAccess.Repos;

public class CriterionRepo : TemporalTableBaseRepo<Criterion>, ICriterionRepo
{
    #region ctors
    public CriterionRepo(ApplicationDBContext context) : base(context) { }
    internal CriterionRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // public override IEnumerable<Criterion> GetAll()
    // {
    //     return Context.Criteria;
    // }

    public IEnumerable<Criterion> GetByCriteriaFilterIds(int[] criteriaFilterIds)
    {
        return Context.Criteria.Where(x => criteriaFilterIds.Any(a => a == x.CriteriaFilterId)).ToList();
    }

    public int Approve(CriterionDto dto)
    {
        var criteria = FindIgnoreQueryFilters(dto.Id);

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

        return SaveChanges();
    }
    #endregion
}
