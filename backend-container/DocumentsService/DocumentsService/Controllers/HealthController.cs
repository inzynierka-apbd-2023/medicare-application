using Microsoft.AspNetCore.Mvc;

namespace DocumentsService.Controllers;

[ApiController]
[Route("healthz")] 
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("ok");
}
