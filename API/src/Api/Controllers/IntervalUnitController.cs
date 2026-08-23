using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.IntervalUnits.Queries.GetIntervalUnitById;
using FinancialPlanner.Application.Features.IntervalUnits.Queries.GetIntervalUnits;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Route("api/IntervalUnit")]
public class IntervalUnitController(
    GetIntervalUnitsQueryHandler getIntervalUnitsQueryHandler,
    GetIntervalUnitByIdQueryHandler getIntervalUnitByIdQueryHandler) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetIntervalUnits(CancellationToken ct)
    {
        var result = await getIntervalUnitsQueryHandler.HandleAsync(new GetIntervalUnitsQuery(), ct);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIntervalUnitById(int id, CancellationToken ct)
    {
        var result = await getIntervalUnitByIdQueryHandler.HandleAsync(new GetIntervalUnitByIdQuery(id), ct);

        return HandleResult(result);
    }
}
