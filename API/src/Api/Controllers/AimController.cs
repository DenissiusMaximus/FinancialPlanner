using FinancialPlanner.Application.Common.Pagination;
using FinancialPlanner.Application.Features.Aims.Commands.AddSourceToAim;
using FinancialPlanner.Application.Features.Aims.Commands.CreateAim;
using FinancialPlanner.Application.Features.Aims.Commands.DeleteAim;
using FinancialPlanner.Application.Features.Aims.Commands.RemoveSourceFromAim;
using FinancialPlanner.Application.Features.Aims.Commands.UpdateAim;
using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Application.Features.Aims.Queries.GetAim;
using FinancialPlanner.Application.Features.Aims.Queries.GetAims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Authorize]
[Route("api/Aim")]
public class AimController(
    CreateAimCommandHandler createAimCommandHandler,
    UpdateAimCommandHandler updateAimCommandHandler,
    DeleteAimCommandHandler deleteAimCommandHandler,
    AddSourceToAimCommandHandler addSourceToAimCommandHandler,
    RemoveSourceFromAimCommandHandler removeSourceFromAimCommandHandler,
    GetAimQueryHandler getAimQueryHandler,
    GetAimsQueryHandler getAimsQueryHandler) : BaseApiController
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAim(int id, CancellationToken ct)
    {
        var result = await getAimQueryHandler.HandleAsync(new GetAimQuery(id), ct);

        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAims([FromQuery] GetAimsQuery query, CancellationToken ct)
    {
        var result = await getAimsQueryHandler.HandleAsync(query, ct);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAim(CreateAimCommand command, CancellationToken ct)
    {
        var result = await createAimCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateAim(int id, UpdateAimCommand command, CancellationToken ct)
    {
        var result = await updateAimCommandHandler.HandleAsync(command with { Id = id }, ct);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAim(int id, CancellationToken ct)
    {
        var result = await deleteAimCommandHandler.HandleAsync(new DeleteAimCommand(id), ct);

        return HandleResult(result);
    }

    [HttpPost("{aimId}/sources/{sourceId}")]
    public async Task<IActionResult> AddSourceToAim(int aimId, int sourceId, CancellationToken ct)
    {
        var result = await addSourceToAimCommandHandler.HandleAsync(new AddSourceToAimCommand(aimId, sourceId), ct);

        return HandleResult(result);
    }

    [HttpDelete("{aimId}/sources/{sourceId}")]
    public async Task<IActionResult> RemoveSourceFromAim(int aimId, int sourceId, CancellationToken ct)
    {
        var result = await removeSourceFromAimCommandHandler.HandleAsync(new RemoveSourceFromAimCommand(aimId, sourceId), ct);

        return HandleResult(result);
    }
}
