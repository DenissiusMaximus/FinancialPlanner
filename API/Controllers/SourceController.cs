using API.Dtos;
using API.Inputs;
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
        return Ok((IReadOnlyCollection<SourceDto>?)await sourceService.GetSources());
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<SourceDto>> GetById(int id)
    {
        return Ok(await sourceService.GetSourceById(id));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SourceDto>> Create(CreateSourceInput input)
    {
        return Ok(await sourceService.CreateSource(input));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<SourceDto>> Update(int id, UpdateSourceInput input)
    {
        return Ok(await sourceService.UpdateSource(id, input));
    }

    [Authorize]
    [HttpPatch("archive/{id}")]
    public async Task<ActionResult<SourceDto>> Archive(int id)
    {
        return Ok(await sourceService.ArchiveSource(id));
    }

    [Authorize]
    [HttpPatch("unarchive/{id}")]
    public async Task<ActionResult<SourceDto>> UnArchive(int id)
    {
        return Ok(await sourceService.UnArchiveSource(id));
    }
}