// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices;

public class ProductService : BaseService<Product, ProductDto>, IProductService
{
    public ProductService(
        IProductRepo repo) : base(repo)
    { }

    protected override ProductDto ToDto(Product entity) => entity.ToDto();

    public override async Task<Result<IEnumerable<ProductDto>>> ReadAll()
    {
        return Result<IEnumerable<ProductDto>>.Success(
            await MainRepo.Context.Products
                .AsNoTracking()
                .Where(x =>
                    x.Proposal == null ||
                    x.Proposal.Status == ProposalStatus.Approved)
                .OrderBy(x => x.Id)
                .Select(CatalogMappings.ProductProjection)
                .ToListAsync());
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetByCriteriaFilterIds(int[] criteriaFilterIds)
    {
        var query = MainRepo.Context.Products
            .AsNoTracking();

        foreach (int id in criteriaFilterIds)
        {
            int criteriaFilterId = id;

            query = query.Where(p =>
                p.ProductCriteriaFilters
                    .Any(x => x.CriteriaFilterId == criteriaFilterId));
        }

        return Result<IEnumerable<ProductDto>>.Success(
                await query
                    .OrderBy(x => x.Id)
                    .Select(CatalogMappings.ProductProjection)
                    .ToListAsync());
    }
}
