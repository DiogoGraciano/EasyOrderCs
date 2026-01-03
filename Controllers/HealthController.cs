using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EasyOrderCs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private static readonly DateTime StartTime = DateTime.UtcNow;

    [HttpGet]
    public IActionResult Check()
    {
        var uptime = DateTime.UtcNow - StartTime;
        return Ok(new
        {
            status = "ok",
            timestamp = DateTime.UtcNow.ToString("O"),
            uptime = uptime.TotalSeconds
        });
    }
}

