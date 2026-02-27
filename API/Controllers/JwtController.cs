
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JwtController(IJwtService jwtService) : ControllerBase
{
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody]string refreshToken)
    {
        var newAccessToken = await jwtService.RefreshToken(refreshToken);

        if (newAccessToken != null)
        {
            return Ok(newAccessToken);
        }

        return BadRequest("Invalid refresh token");
    }
}