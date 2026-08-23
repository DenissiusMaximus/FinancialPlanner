using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.Currencies.Queries.GetCurrencies;
using FinancialPlanner.Application.Features.Currencies.Queries.GetCurrencyById;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Route("api/Currency")]
public class CurrencyController(
    GetCurrenciesQueryHandler getCurrenciesQueryHandler,
    GetCurrencyByIdQueryHandler getCurrencyByIdQueryHandler) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAllCurrencies(CancellationToken ct)
    {
        var result = await getCurrenciesQueryHandler.HandleAsync(new GetCurrenciesQuery(), ct);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCurrencyById(int id, CancellationToken ct)
    {
        var result = await getCurrencyByIdQueryHandler.HandleAsync(new GetCurrencyByIdQuery(id), ct);

        return HandleResult(result);
    }
}
