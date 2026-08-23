using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.TransactionTypes.Queries.GetTransactionTypeById;
using FinancialPlanner.Application.Features.TransactionTypes.Queries.GetTransactionTypes;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Route("api/TransactionType")]
public class TransactionTypeController(
    GetTransactionTypesQueryHandler getTransactionTypesQueryHandler,
    GetTransactionTypeByIdQueryHandler getTransactionTypeByIdQueryHandler) : BaseApiController
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionType(int id, CancellationToken ct)
    {
        var result = await getTransactionTypeByIdQueryHandler.HandleAsync(new GetTransactionTypeByIdQuery(id), ct);

        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactionTypes(CancellationToken ct)
    {
        var result = await getTransactionTypesQueryHandler.HandleAsync(new GetTransactionTypesQuery(), ct);

        return HandleResult(result);
    }
}
