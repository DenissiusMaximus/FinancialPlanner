using FinancialPlanner.Application.Features.Jwt.Commands.RefreshAccessToken;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Route("api/Jwt")]
public class JwtController(RefreshAccessTokenCommandHandler refreshAccessTokenCommandHandler) : BaseApiController
{
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken, CancellationToken ct)
    {
        var result = await refreshAccessTokenCommandHandler.HandleAsync(new RefreshAccessTokenCommand(refreshToken), ct);

        return HandleResult(result);
    }
}
