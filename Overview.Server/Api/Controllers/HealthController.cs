using Microsoft.AspNetCore.Mvc;
using Overview.Server.Api.Contracts;

namespace Overview.Server.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse
        {
            Status = "ok",
            Service = "overview-server"
        });
    }
}
