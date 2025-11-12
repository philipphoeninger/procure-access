namespace API.ProcureAccess.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
   [HttpGet]
   public IActionResult Get() => Ok(new { status = "ok", time = DateTime.UtcNow });
}

