using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.Frequencies.Commands.CreateFrequency;
using FinancialPlanner.Application.Features.Frequencies.Commands.DeleteFrequency;
using FinancialPlanner.Application.Features.Frequencies.Commands.UpdateFrequency;
using FinancialPlanner.Application.Features.Frequencies.Queries.GetFrequencies;
using FinancialPlanner.Application.Features.Frequencies.Queries.GetFrequencyById;
using FinancialPlanner.Application.Features.Frequencies.Queries.GetUserFrequencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Authorize]
[Route("api/Frequency")]
public class FrequencyController(
    CreateFrequencyCommandHandler createFrequencyCommandHandler,
    UpdateFrequencyCommandHandler updateFrequencyCommandHandler,
    DeleteFrequencyCommandHandler deleteFrequencyCommandHandler,
    GetFrequenciesQueryHandler getFrequenciesQueryHandler,
    GetUserFrequenciesQueryHandler getUserFrequenciesQueryHandler,
    GetFrequencyByIdQueryHandler getFrequencyByIdQueryHandler) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetFrequencies(CancellationToken ct)
    {
        var result = await getFrequenciesQueryHandler.HandleAsync(new GetFrequenciesQuery(), ct);

        return HandleResult(result);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserFrequencies(CancellationToken ct)
    {
        var result = await getUserFrequenciesQueryHandler.HandleAsync(new GetUserFrequenciesQuery(), ct);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFrequency(int id, CancellationToken ct)
    {
        var result = await getFrequencyByIdQueryHandler.HandleAsync(new GetFrequencyByIdQuery(id), ct);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFrequency(CreateFrequencyCommand command, CancellationToken ct)
    {
        var result = await createFrequencyCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateFrequency(int id, UpdateFrequencyCommand command, CancellationToken ct)
    {
        var result = await updateFrequencyCommandHandler.HandleAsync(command with { Id = id }, ct);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFrequency(int id, CancellationToken ct)
    {
        var result = await deleteFrequencyCommandHandler.HandleAsync(new DeleteFrequencyCommand(id), ct);

        return HandleResult(result);
    }
}
