using API.Dtos;
using API.Filter;
using API.Inputs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [TypeFilter(typeof(ValidationFilter<CreateUserInput>))]
    [HttpPost("register")]
    public async Task<ActionResult<AuthUserDto?>> Register(CreateUserInput input)
    {
        var result = await userService.CreateUser(input.Name, input.Email, input.Password);

        if (result == null)
            return Unauthorized();

        return Ok(result);
    }

    [TypeFilter(typeof(ValidationFilter<LoginUserInput>))]
    [HttpPost("login")]
    public async Task<ActionResult<AuthUserDto?>> Login(LoginUserInput input)
    {
        var result = await userService.LoginUser(input.Email, input.Password);

        if (result == null)
            return Unauthorized();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("test")]
    public async Task<ActionResult> Test()
    {
        return Ok("Test endpoint is working!");
    }
}   
