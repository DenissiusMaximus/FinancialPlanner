using System;
using API.Extensions;
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
        var userId = User.GetRequiredUserId();

        return await frequencyService.GetFrequencies(userId);
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<ActionResult<List<FrequencyDto>>> GetUserFrequencies()
    {
        var userId = User.GetRequiredUserId();

        return await frequencyService.GetUserFrequencies(userId);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<FrequencyDto>> GetFrequency(int id)
    {
        var userId = User.GetRequiredUserId();

        var frequency = await frequencyService.GetFrequency(id, userId);

        return frequency!;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<FrequencyDto>> CreateFrequency(FrequencyInput frequency)
    {
        var userId = User.GetRequiredUserId();

        var createdFrequency = await frequencyService.CreateFrequency(frequency, userId);

        return createdFrequency!;
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<FrequencyDto>> UpdateFrequency(int id, FrequencyInput frequency)
    {
        var userId = User.GetRequiredUserId();

        var updatedFrequency = await frequencyService.UpdateFrequency(frequency, id, userId);

        return updatedFrequency!;
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteFrequency(int id)
    {
        var userId = User.GetRequiredUserId();

        var deleted = await frequencyService.DeleteFrequency(id, userId);

        return deleted;
    }
}
