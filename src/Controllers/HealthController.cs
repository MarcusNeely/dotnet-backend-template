using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>Health check endpoints</summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>Returns API health status</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() =>
        Ok(new { status = "ok", timestamp = DateTime.UtcNow, version = "1.0.0" });
}
