using API.Dtos;
using API.Inputs;
using API.Services.Aim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AimController(IAimService aimService) : ControllerBase
{
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<AimDto>> GetAim(int id)
    {
        var aim = await aimService.GetAim(id);

        return Ok(aim);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<AimDto>> GetAims([FromQuery]GetAimsInput input)
    {
        var aims = await aimService.GetAims(input);

        return Ok(aims);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AimDto>> CreateAim(CreateAimInput input)
    {
        var aim = await aimService.CreateAim(input);

        return Ok(aim);
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<AimDto>> UpdateAim(int id, UpdateAimInput input)
    {
        var aim = await aimService.UpdateAim(id, input);

        return Ok(aim);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAim(int id)
    {
        var result = await aimService.DeleteAim(id);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{aimId}/sources/{sourceId}")]
    public async Task<ActionResult<SourceDtoLookup>> AddSourceToAim(int aimId, int sourceId)
    {
        var source = await aimService.AddSourceToAim(aimId, sourceId);

        return Ok(source);
    }

    [Authorize]
    [HttpDelete("{aimId}/sources/{sourceId}")]
    public async Task<ActionResult<bool>> RemoveSourceFromAim(int aimId, int sourceId)
    {
        var result = await aimService.RemoveSourceFromAim(aimId, sourceId);

        return Ok(result);
    }
}
