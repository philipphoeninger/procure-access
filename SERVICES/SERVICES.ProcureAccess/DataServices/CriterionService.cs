// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

public class CriterionService : BaseService<Criterion, CriterionDto>, ICriterionService
{
    public CriterionService(
        ICriterionRepo repo) : base(repo)
    { }

    protected override CriterionDto ToDto(Criterion entity) => entity.ToDto();

    public override async Task<Result<IEnumerable<CriterionDto>>?> ReadAllAsync()
    {
        return Result<IEnumerable<CriterionDto>>.Success(
            await MainRepo.Context.Criteria
                .AsNoTracking()
                .Where(x => x.Proposal == null ||
                            x.Proposal.Status == ProposalStatus.Approved)
                .OrderBy(x => x.Id)
                .Select(CatalogMappings.CriterionProjection)
                .ToListAsync());
    }

    public async Task<IEnumerable<CriterionDto>> GetByCriteriaFilterIdsAsync(int[] criteriaFilterIds)
    {
        return await MainRepo.Context.Criteria
            .AsNoTracking()
            .Where(x => criteriaFilterIds.Contains(x.CriteriaFilterId))
            .OrderBy(x => x.Id)
            .Select(CatalogMappings.CriterionProjection)
            .ToListAsync();
    }
}
