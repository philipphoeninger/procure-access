// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

public class FilterTypeService : BaseService<FilterType, FilterTypeDto>, IFilterTypeService
{
    public FilterTypeService(IFilterTypeRepo repo) : base(repo)
    {
    }

    protected override FilterTypeDto ToDto(FilterType entity) => entity.ToDto();
}
