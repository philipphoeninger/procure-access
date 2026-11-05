// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices.Base;

/// <summary>
/// Read side of the legacy generic CRUD services. The writes went with the generic CRUD
/// endpoints (S3-07, #90); each service maps its entity in <see cref="ToDto"/> (ADR-015).
/// </summary>
public abstract class BaseService<TEntity, TDto> : IBaseService<TEntity, TDto>
    where TEntity : BaseEntity, new()
    where TDto : BaseDto, new()
{
    public IBaseRepo<TEntity> MainRepo { get; set; }

    #region ctors
    protected BaseService(IBaseRepo<TEntity> repo)
    {
        MainRepo = repo;
    }
    #endregion

    #region methods
    public virtual async Task<Result<TDto?>> Read(int id)
    {
        TEntity? entity = MainRepo.Find(id);
        if (entity is null) return null; //gate
        return Result<TDto?>.Success(ToDto(entity));
    }

    public virtual async Task<Result<IEnumerable<TDto>>> ReadAll()
    {
        List<TEntity> entities = MainRepo.GetAll().ToList();
        List<TDto> dtos = new List<TDto>();
        entities.ForEach(x => dtos.Add(ToDto(x)));
        return Result<IEnumerable<TDto>>.Success(dtos);
    }

    /// <summary>The service's map from entity to DTO (CatalogMappings, AccountMappings).</summary>
    protected abstract TDto ToDto(TEntity entity);
    #endregion
}
