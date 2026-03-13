namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UICustomizationController : BaseCrudController<UICustomization, UICustomizationController>
{
    protected readonly IUICustomizationRepo Repo;

    public UICustomizationController(IAppLogging<UICustomizationController> logger, IUICustomizationRepo repo) : base(logger, repo)
    {
        Repo = repo;
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public override ActionResult<IEnumerable<UICustomization>> GetAll()
    {
        // TODO: authorize admin user for this call
        return Unauthorized();
    }
}
