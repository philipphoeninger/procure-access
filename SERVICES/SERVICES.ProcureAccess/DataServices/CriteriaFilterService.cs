// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

public class CriteriaFilterService : BaseService<CriteriaFilter, CriteriaFilterDto>, ICriteriaFilterService
{
    public CriteriaFilterService(
        ICriteriaFilterRepo repo) : base(repo)
    { }

    protected override CriteriaFilterDto ToDto(CriteriaFilter entity) => entity.ToDto();

    public override async Task<Result<IEnumerable<CriteriaFilterDto>>?> ReadAllAsync()
    {
        return Result<IEnumerable<CriteriaFilterDto>>.Success(
            await MainRepo.Context.CriteriaFilters
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(CatalogMappings.CriteriaFilterProjection)
                .ToListAsync());
    }
}
