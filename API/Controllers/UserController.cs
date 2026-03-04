using API.Dtos;
using API.Inputs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthUserDto?>> Register(CreateUserInput input)
    {
        var result = await userService.CreateUser(input.Name, input.Email, input.Password);

        if (result == null)
            return Unauthorized();

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthUserDto?>> Login(LoginUserInput input)
    {
        var result = await userService.LoginUser(input.Email, input.Password);

        if (result == null)
            return Unauthorized();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] string refreshToken)
    {
        var result = await userService.LogoutUser(refreshToken);

        if (!result)
            return BadRequest("Failed to logout");

        return Ok();
    }
}   
