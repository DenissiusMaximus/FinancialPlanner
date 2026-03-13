using API.Dtos;
using API.Extensions;
using API.Services.Source;
using Microsoft.AspNetCore.Authorization;
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

        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SourceDto>> Create(CreateSourceInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.CreateSource(input, userId);

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<SourceDto>> Update(int id, UpdateSourceInput input)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.UpdateSource(id, input, userId);

        return Ok(result);
    }

    [Authorize]
    [HttpPatch("archive/{id}")]
    public async Task<ActionResult<SourceDto>> Archive(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.ArchiveSource(id, userId);

        return Ok(result);
    }

    [Authorize]
    [HttpPatch("unarchive/{id}")]
    public async Task<ActionResult<SourceDto>> UnArchive(int id)
    {
        var userId = User.GetRequiredUserId();

        var result = await sourceService.UnArchiveSource(id, userId);

        return Ok(result);
    }
}