// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

public class CriteriaFilterService : BaseService<CriteriaFilter, CriteriaFilterDto>, ICriteriaFilterService
{
    public CriteriaFilterService(
        ICriteriaFilterRepo repo,
        IMapper mapper) : base(repo, mapper)
    { }

    public override async Task<Result<IEnumerable<CriteriaFilterDto>>> ReadAll()
    {
        return Result<IEnumerable<CriteriaFilterDto>>.Success(
            await MainRepo.Context.CriteriaFilters
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .ProjectTo<CriteriaFilterDto>(Mapper.ConfigurationProvider)
                .ToListAsync());
    }
}
