using FinancialPlanner.Application.Common.Pagination;
using FinancialPlanner.Application.Features.Transactions.Commands.CreateTransaction;
using FinancialPlanner.Application.Features.Transactions.Commands.DeleteTransaction;
using FinancialPlanner.Application.Features.Transactions.Commands.UpdateTransaction;
using FinancialPlanner.Application.Features.Transactions.Dtos;
using FinancialPlanner.Application.Features.Transactions.Queries.GetTransactionById;
using FinancialPlanner.Application.Features.Transactions.Queries.GetTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

[Authorize]
[Route("api/Transaction")]
public class TransactionController(
    CreateTransactionCommandHandler createTransactionCommandHandler,
    UpdateTransactionCommandHandler updateTransactionCommandHandler,
    DeleteTransactionCommandHandler deleteTransactionCommandHandler,
    GetTransactionsQueryHandler getTransactionsQueryHandler,
    GetTransactionByIdQueryHandler getTransactionByIdQueryHandler) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] GetTransactionsQuery query, CancellationToken ct)
    {
        var result = await getTransactionsQueryHandler.HandleAsync(query, ct);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(int id, CancellationToken ct)
    {
        var result = await getTransactionByIdQueryHandler.HandleAsync(new GetTransactionByIdQuery(id), ct);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(CreateTransactionCommand command, CancellationToken ct)
    {
        var result = await createTransactionCommandHandler.HandleAsync(command, ct);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id, CancellationToken ct)
    {
        var result = await deleteTransactionCommandHandler.HandleAsync(new DeleteTransactionCommand(id), ct);

        return HandleResult(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, UpdateTransactionCommand command, CancellationToken ct)
    {
        var result = await updateTransactionCommandHandler.HandleAsync(command with { Id = id }, ct);

        return HandleResult(result);
    }
}
