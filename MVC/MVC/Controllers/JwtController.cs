using API.Services;
using API.Services.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JwtController(IJwtService jwtService) : ControllerBase
{
    [HttpPost("refreshToken")]
    public async Task<ActionResult<string>> RefreshToken([FromBody] string refreshToken)
    {
        var newAccessToken = await jwtService.RefreshToken(refreshToken);

        if (newAccessToken == null)
            return BadRequest("Invalid refresh token");

        return Ok(newAccessToken);
    }
}