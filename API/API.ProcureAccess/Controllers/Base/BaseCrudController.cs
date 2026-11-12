// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers.Base;

/// <summary>
/// Read-only base of the legacy catalog and proposal controllers. The generic <c>POST</c>,
/// <c>PUT</c> and <c>DELETE</c> are gone (S3-07, owner decision O-S3-4 (c), #90): they accepted
/// anonymous requests, and the catalog changes only through the CLI import (SPEC SD20).
/// Its routes stay unversioned (<c>api/[controller]</c>; the derived controllers declare the same):
/// only the new module endpoints live under <c>/api/v1</c> (owner, S3-19, #153).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BaseCrudController<TEntity, TDto> : ControllerBase
    where TEntity : BaseEntity, new()
    where TDto : BaseDto, new()
{
    protected readonly IBaseService<TEntity, TDto> MainService;

    protected BaseCrudController(IBaseService<TEntity, TDto> service)
    {
        MainService = service;
    }

    /// <summary>
    /// Gets a single record
    /// </summary>
    /// <param name="id">Primary key of the record</param>
    /// <returns>Single record</returns>
    [HttpGet("{id}")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(204, "No content")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<ActionResult<TDto>> GetOneAsync(int id)
    {
        Result<TDto?>? dto = await MainService.ReadAsync(id);

        return dto == null ? NoContent() : Ok(dto);
    }

    /// <summary>
    /// Gets all records
    /// </summary>
    /// <returns>All records</returns>
    [HttpGet]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAllAsync()
    {
        return Ok(await MainService.ReadAllAsync());
    }
}
