using API.Inputs;
using API.Services.Aim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AimController(IAimService aimService) : ControllerBase
{
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAim(int id)
    {
        var aim = await aimService.GetAim(id);

        return Ok(aim);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAims()
    {
        var aims = await aimService.GetAims();

        return Ok(aims);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAim(CreateAimInput input)
    {
        var aim = await aimService.CreateAim(input);

        return Ok(aim);
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateAim(int id, UpdateAimInput input)
    {
        var aim = await aimService.UpdateAim(id, input);

        return Ok(aim);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAim(int id)
    {
        var result = await aimService.DeleteAim(id);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{aimId}/sources/{sourceId}")]
    public async Task<IActionResult> AddSourceToAim(int aimId, int sourceId)
    {
        var source = await aimService.AddSourceToAim(aimId, sourceId);

        return Ok(source);
    }

    [Authorize]
    [HttpDelete("{aimId}/sources/{sourceId}")]
    public async Task<IActionResult> RemoveSourceFromAim(int aimId, int sourceId)
    {
        var result = await aimService.RemoveSourceFromAim(aimId, sourceId);

        return Ok(result);
    }
}
