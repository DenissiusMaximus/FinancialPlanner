using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.Sources.Commands.ArchiveSource;
using FinancialPlanner.Application.Features.Sources.Commands.CreateSource;
using FinancialPlanner.Application.Features.Sources.Commands.DeleteSource;
using FinancialPlanner.Application.Features.Sources.Commands.UnarchiveSource;
using FinancialPlanner.Application.Features.Sources.Commands.UpdateSource;
using FinancialPlanner.Application.Features.Sources.Dtos;
using FinancialPlanner.Application.Features.Sources.Queries.GetSourceById;
using FinancialPlanner.Application.Features.Sources.Queries.GetSources;
using FinancialPlanner.Application.Features.Sources.Queries.GetSourceSummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Authorize]
[Route("api/Source")]
public class SourceController(
    CreateSourceCommandHandler createSourceCommandHandler,
    UpdateSourceCommandHandler updateSourceCommandHandler,
    DeleteSourceCommandHandler deleteSourceCommandHandler,
    ArchiveSourceCommandHandler archiveSourceCommandHandler,
    UnarchiveSourceCommandHandler unarchiveSourceCommandHandler,
    GetSourcesQueryHandler getSourcesQueryHandler,
    GetSourceByIdQueryHandler getSourceByIdQueryHandler,
    GetSourceSummaryQueryHandler getSourceSummaryQueryHandler) : BaseApiController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSourceSummary(CancellationToken ct)
    {
        var result = await getSourceSummaryQueryHandler.HandleAsync(new GetSourceSummaryQuery(), ct);

        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetSources(CancellationToken ct)
    {
        var result = await getSourcesQueryHandler.HandleAsync(new GetSourcesQuery(), ct);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSource(int id, CancellationToken ct)
    {
        var result = await deleteSourceCommandHandler.HandleAsync(new DeleteSourceCommand(id), ct);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSourceById(int id, CancellationToken ct)
    {
        var result = await getSourceByIdQueryHandler.HandleAsync(new GetSourceByIdQuery(id), ct);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSource(CreateSourceCommand command, CancellationToken ct)
    {
        var result = await createSourceCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateSource(int id, UpdateSourceCommand command, CancellationToken ct)
    {
        var result = await updateSourceCommandHandler.HandleAsync(command with { Id = id }, ct);

        return HandleResult(result);
    }

    [HttpPatch("archive/{id}")]
    public async Task<IActionResult> ArchiveSource(int id, CancellationToken ct)
    {
        var result = await archiveSourceCommandHandler.HandleAsync(new ArchiveSourceCommand(id), ct);

        return HandleResult(result);
    }

    [HttpPatch("unarchive/{id}")]
    public async Task<IActionResult> UnArchiveSource(int id, CancellationToken ct)
    {
        var result = await unarchiveSourceCommandHandler.HandleAsync(new UnarchiveSourceCommand(id), ct);

        return HandleResult(result);
    }
}
