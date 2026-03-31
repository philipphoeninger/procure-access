namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseCrudController<Product, ProductDto, ProductsController>
{
    private readonly IProductService _productService;

    public ProductsController(
        IAppLogging<ProductsController> logger, 
        IProductService service) : base(logger, service)
    {
        _productService = service;
    }

    /// <summary>
    /// Updates & approves a single record
    /// </summary>
    /// <remarks>
    /// <pre>
    /// </pre>
    /// </remarks>
    /// <param name="dto">Dto Entity to update</param>
    /// <returns>Single record</returns>
    [HttpPut("approve")]
    [ApiVersion("1.0")]
    // [Authorize(Roles = $"{Roles.Admin},{Roles.Approver}")]
    [AuthApproverAttribute]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public ActionResult<bool> ApproveOne(ProductDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        int success = -1;
        try
        {
            success = _productService.Approve(dto);
        }
        catch (CustomException ex)
        {
            // TODO: handle more gracefully
            return BadRequest(ex);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok(Convert.ToBoolean(success));
    }
}
