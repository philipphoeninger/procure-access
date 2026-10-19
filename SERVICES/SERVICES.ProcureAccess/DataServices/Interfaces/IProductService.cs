// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IProductService : IBaseService<Product, ProductDto>
{
    Task<Result<IEnumerable<ProductDto>>> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
