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
    /// Gets all records, that have the provided Criteria Filter Id(s).
    /// </summary>
    /// <param name="criteriaFilterIds">To filter Criterias by Criteria Filter Ids.</param>
    /// <returns>Criteria</returns>
    [HttpGet("byCriteriaFilterIds")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(204, "No content")]
    [SwaggerResponse(400, "The request was invalid")]
    public ActionResult<List<ProductDto>> ByCriteriaFilterIds([FromQuery] int[] criteriaFilterIds)
    {
        var filteredProducts = _productService.GetByCriteriaFilterIds(criteriaFilterIds);
        return Ok(filteredProducts);
    }
}
