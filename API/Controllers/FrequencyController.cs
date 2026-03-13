using API.Inputs;
using API.Models;
using API.Services.Frequency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FrequencyController(IFrequencyService frequencyService) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<FrequencyDto>>> GetFrequencies()
    {
        return Ok(await frequencyService.GetFrequencies());
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<ActionResult<List<FrequencyDto>>> GetUserFrequencies()
    {
        return Ok(await frequencyService.GetUserFrequencies());
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<FrequencyDto>> GetFrequency(int id)
    {
        return Ok(await frequencyService.GetFrequency(id));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<FrequencyDto>> CreateFrequency(FrequencyInput frequency)
    {
        return Ok(await frequencyService.CreateFrequency(frequency));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<FrequencyDto>> UpdateFrequency(int id, FrequencyInput frequency)
    {
        return Ok(await frequencyService.UpdateFrequency(frequency, id));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteFrequency(int id)
    {
        return Ok(await frequencyService.DeleteFrequency(id));
    }
}
