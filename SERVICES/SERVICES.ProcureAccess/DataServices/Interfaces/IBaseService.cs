// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IBaseService<TEntity, TDto>
    where TEntity : BaseEntity, new()
    where TDto : BaseDto, new()
{
    IBaseRepo<TEntity> MainRepo { get; set; }
    Task<Result<TDto?>?> ReadAsync(int id);
    Task<Result<IEnumerable<TDto>>?> ReadAllAsync();
}
