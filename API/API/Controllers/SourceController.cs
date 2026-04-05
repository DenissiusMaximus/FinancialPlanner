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
    [HttpGet("summary")]
    public async Task<ActionResult<SourceSummaryDto>> GetSummary()
    {
        return Ok(await sourceService.GetSourceSummary());
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SourceDtoLookup>>> Get()
    {
        return Ok(await sourceService.GetSources());
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<SourceDtoDetailed>> GetById(int id)
    {
        return Ok(await sourceService.GetSourceById(id));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SourceDtoLookup>> Create(CreateSourceInput input)
    {
        return Ok(await sourceService.CreateSource(input));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<SourceDtoLookup>> Update(int id, UpdateSourceInput input)
    {
        return Ok(await sourceService.UpdateSource(id, input));
    }

    [Authorize]
    [HttpPatch("archive/{id}")]
    public async Task<ActionResult<SourceDtoLookup>> Archive(int id)
    {
        return Ok(await sourceService.ArchiveSource(id));
    }

    [Authorize]
    [HttpPatch("unarchive/{id}")]
    public async Task<ActionResult<SourceDtoLookup>> UnArchive(int id)
    {
        return Ok(await sourceService.UnArchiveSource(id));
    }
}