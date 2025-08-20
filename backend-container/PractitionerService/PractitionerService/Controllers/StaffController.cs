using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/staff")]
public class StaffController : ControllerBase
{
    private static readonly object Message = new
    {
        message = "The /api/practitioner/staff API is deprecated. Use /api/practitioner/doctors and /api/practitioner/catalog instead."
    };

    [HttpGet]
    public IActionResult Get() => StatusCode(410, Message);

    [HttpGet("{id}")]
    public IActionResult GetById(string id) => StatusCode(410, Message);

    [HttpGet("role/{role}")]
    public IActionResult GetByRole(string role) => StatusCode(410, Message);

    [HttpPost]
    [Authorize]
    public IActionResult Create() => StatusCode(410, Message);

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(string id) => StatusCode(410, Message);

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(string id) => StatusCode(410, Message);

    [HttpGet("specializations")]
    public IActionResult Specializations() => StatusCode(410, Message);

    [HttpGet("services")]
    public IActionResult Services() => StatusCode(410, Message);
}
