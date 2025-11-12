namespace API.ProcureAccess.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountsController : ControllerBase
{
    protected readonly IAppLogging<AccountsController> Logger;
    protected readonly HttpContext? HContext;
    protected readonly UserManager<User> UserManager;

    public AccountsController(
        IAppLogging<AccountsController> logger,
        IHttpContextAccessor hAccess,
        UserManager<User> userManager)
    {
        Logger = logger;
        HContext = hAccess.HttpContext;
        UserManager = userManager;
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
    public async Task<IResult> GetOneAsync(int id)
    {
        ClaimsPrincipal cp = HContext!.User;
        string userID = cp.Claims.First(x => x.Type == "UserID").Value;
        User? userDetails = await UserManager.FindByIdAsync(userID);

        return Results.Ok(
            new
            {
                userDetails?.Email
            });
    }
}

