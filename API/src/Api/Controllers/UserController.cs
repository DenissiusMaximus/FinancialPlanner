using FinancialPlanner.Application.Features.Users.Commands.LoginUser;
using FinancialPlanner.Application.Features.Users.Commands.LogoutUser;
using FinancialPlanner.Application.Features.Users.Commands.RegisterUser;
using FinancialPlanner.Application.Features.Users.Dtos;
using FinancialPlanner.Application.Features.Users.Queries.GetCurrentUser;
using FinancialPlanner.Application.Features.Users.Queries.IsEmailAvailable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Route("api/User")]
public class UserController(
    RegisterUserCommandHandler registerUserCommandHandler,
    LoginUserCommandHandler loginUserCommandHandler,
    LogoutUserCommandHandler logoutUserCommandHandler,
    GetCurrentUserQueryHandler getCurrentUserQueryHandler,
    IsEmailAvailableQueryHandler isEmailAvailableQueryHandler) : BaseApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken ct)
    {
        var result = await registerUserCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserCommand command, CancellationToken ct)
    {
        var result = await loginUserCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken, CancellationToken ct)
    {
        var result = await logoutUserCommandHandler.HandleAsync(new LogoutUserCommand(refreshToken), ct);

        return HandleResult(result);
    }

    [HttpGet("email-available")]
    public async Task<IActionResult> IsEmailAvailable(string email, CancellationToken ct)
    {
        var result = await isEmailAvailableQueryHandler.HandleAsync(new IsEmailAvailableQuery(email), ct);

        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await getCurrentUserQueryHandler.HandleAsync(new GetCurrentUserQuery(), ct);

        return HandleResult(result);
    }
}
