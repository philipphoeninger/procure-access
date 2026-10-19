// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface ICriterionService : IBaseService<Criterion, CriterionDto>
{
    Task<IEnumerable<CriterionDto>> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
