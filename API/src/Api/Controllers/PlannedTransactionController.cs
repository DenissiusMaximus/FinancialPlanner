using FinancialPlanner.Application.Common.Pagination;
using FinancialPlanner.Application.Features.PlannedTransactions.Commands.CreatePlannedTransaction;
using FinancialPlanner.Application.Features.PlannedTransactions.Commands.DeletePlannedTransaction;
using FinancialPlanner.Application.Features.PlannedTransactions.Commands.UpdatePlannedTransaction;
using FinancialPlanner.Application.Features.PlannedTransactions.Dtos;
using FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactionById;
using FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Authorize]
[Route("api/PlannedTransaction")]
public class PlannedTransactionController(
    CreatePlannedTransactionCommandHandler createPlannedTransactionCommandHandler,
    UpdatePlannedTransactionCommandHandler updatePlannedTransactionCommandHandler,
    DeletePlannedTransactionCommandHandler deletePlannedTransactionCommandHandler,
    GetPlannedTransactionsQueryHandler getPlannedTransactionsQueryHandler,
    GetPlannedTransactionByIdQueryHandler getPlannedTransactionByIdQueryHandler) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetPlannedTransactions([FromQuery] GetPlannedTransactionsQuery query, CancellationToken ct)
    {
        var result = await getPlannedTransactionsQueryHandler.HandleAsync(query, ct);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlannedTransactionById(int id, CancellationToken ct)
    {
        var result = await getPlannedTransactionByIdQueryHandler.HandleAsync(new GetPlannedTransactionByIdQuery(id), ct);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlannedTransaction(CreatePlannedTransactionCommand command, CancellationToken ct)
    {
        var result = await createPlannedTransactionCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlannedTransaction(int id, CancellationToken ct)
    {
        var result = await deletePlannedTransactionCommandHandler.HandleAsync(new DeletePlannedTransactionCommand(id), ct);

        return HandleResult(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdatePlannedTransaction(int id, UpdatePlannedTransactionCommand command, CancellationToken ct)
    {
        var result = await updatePlannedTransactionCommandHandler.HandleAsync(command with { Id = id }, ct);

        return HandleResult(result);
    }
}
