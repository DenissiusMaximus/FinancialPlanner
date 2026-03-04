using API.Dtos;
using API.Extensions;
using API.Filter;
using API.Services.Source;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SourceController(ISourceService sourceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SourceDto>>> Get()
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.GetSources(userId);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SourceDto>> GetById(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.GetSourceById(id, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [TypeFilter(typeof(ValidationFilter<CreateSourceInput>))]
    [HttpPost]
    public async Task<ActionResult<SourceDto>> Create(CreateSourceInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.CreateSource(input, userId);

        if (result == null)
            return BadRequest("Failed to create source");

        return Ok(result);
    }

    [TypeFilter(typeof(ValidationFilter<UpdateSourceInput>))]
    [HttpPut("{id}")]
    public async Task<ActionResult<SourceDto>> Update(int id, UpdateSourceInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.UpdateSource(id, input, userId);

        if (result == null)
            return BadRequest("Failed to update source");

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.DeleteSource(id, userId);

        if (!result)
            return BadRequest("Failed to delete source");

        return Ok();
    }
}