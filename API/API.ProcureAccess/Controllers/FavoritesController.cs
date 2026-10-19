// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// namespace API.ProcureAccess.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class FavoritesController : BaseCrudController<Favorite, FavoritesController>
// {
//     protected readonly IFavoriteRepo Repo;

//     public FavoritesController(IAppLogging<FavoritesController> logger, IFavoriteRepo repo) : base(logger, repo)
//     {
//         Repo = repo;
//     }

//     /// <summary>
//     /// Gets all records
//     /// </summary>
//     /// <returns>All records</returns>
//     [HttpGet]
//     [ApiVersion("1.0")]
//     [Produces("application/json")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status400BadRequest)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//     [SwaggerResponse(200, "The execution was successful")]
//     [SwaggerResponse(400, "The request was invalid")]
//     [SwaggerResponse(401, "Unauthorized access attempted")]
//     public override ActionResult<IEnumerable<Favorite>> GetAll()
//     {
//         string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//         if (userId == null) return Unauthorized();
//         return Ok(Repo.GetAll(userId));
//     }
// }
