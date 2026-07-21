using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace tickethub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // This endpoint is accessible to anyone without authentication
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok("This is a public endpoint accessible to everyone.");
    }

    // This endpoint requires a valid JWT from Keycloak
    [Authorize]
    [HttpGet("secure")]
    public IActionResult SecureEndpoint()
    {
        // You can access user claims via the User property
        var username = User.FindFirst("preferred_username")?.Value ?? "unknown";
        
        return Ok(new 
        { 
            message = "This is a secured endpoint.",
            user = username 
        });
    }
}