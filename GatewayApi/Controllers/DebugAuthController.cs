using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugAuthController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public() => Ok("Public OK");

    [HttpGet("private")]
    [Authorize]
    public IActionResult Private() => Ok("Private OK ✅");
}
