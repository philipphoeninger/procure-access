// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

public class CriterionService : BaseService<Criterion, CriterionDto>, ICriterionService
{
    public CriterionService(
        ICriterionRepo repo,
        IMapper mapper) : base(repo, mapper)
    { }

    public override async Task<Result<IEnumerable<CriterionDto>>> ReadAll()
    {
        return Result<IEnumerable<CriterionDto>>.Success(
            await MainRepo.Context.Criteria
                .AsNoTracking()
                .Where(x => x.Proposal == null ||
                            x.Proposal.Status == ProposalStatus.Approved)
                .OrderBy(x => x.Id)
                .ProjectTo<CriterionDto>(Mapper.ConfigurationProvider)
                .ToListAsync());
    }

    public async Task<IEnumerable<CriterionDto>> GetByCriteriaFilterIds(int[] criteriaFilterIds)
    {
        return await MainRepo.Context.Criteria
            .AsNoTracking()
            .Where(x => criteriaFilterIds.Contains(x.CriteriaFilterId))
            .OrderBy(x => x.Id)
            .ProjectTo<CriterionDto>(Mapper.ConfigurationProvider)
            .ToListAsync();
    }
}
