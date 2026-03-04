using API.Dtos;
using API.Extensions;
using API.Services.Source;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SourceController(ISourceService sourceService) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SourceDto>>> Get()
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.GetSources(userId);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<SourceDto>> GetById(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.GetSourceById(id, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SourceDto>> Create(CreateSourceInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.CreateSource(input, userId);

        if (result == null)
            return BadRequest("Failed to create source");

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<SourceDto>> Update(int id, UpdateSourceInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.UpdateSource(id, input, userId);

        if (result == null)
            return BadRequest("Failed to update source");

        return Ok(result);
    }

    [Authorize]
    [HttpPut("archive/{id}")]
    public async Task<ActionResult> Archive(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.ArchiveSource(id, userId);

        if (result == null)
            return BadRequest("Failed to archive source");

        return Ok(result);
    }

    [Authorize]
    [HttpPost("unarchive/{id}")]
    public async Task<ActionResult> UnArchive(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.UnArchiveSource(id, userId);

        if (result == null)
            return BadRequest("Failed to unarchive source");

        return Ok(result);
    }
}