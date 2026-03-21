using API.Services.Aim;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AimController(IAimService aimService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAim(int id)
    {
        var aim = await aimService.GetAim(id);

        return Ok(aim);
    }

    [HttpGet]
    public async Task<IActionResult> GetAims()
    {
        var aims = await aimService.GetAims();

        return Ok(aims);
    }
}
