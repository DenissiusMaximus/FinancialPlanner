using API.Models;
using API.Services;
using API.Services.PlannedTransaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlannedTransactionController(IPlannedTransactionService transactionService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<PlannedTransactionDto>>> GetUsersTransactions([FromQuery]GetUserPlannedTransactionsInput input)
    {
        return Ok(await transactionService.GetUsersPlannedTransactions(input));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<PlannedTransactionDto>> GetTransactionById(int id)
    {
        return Ok(await transactionService.GetPlannedTransactionById(id));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PlannedTransactionDto>> CreateTransaction(CreatePlannedTransactionInput transactionInput)
    {
        return Ok(await transactionService.CreatePlannedTransaction(transactionInput));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteTransaction(int id)
    {
        return Ok(await transactionService.DeletePlannedTransaction(id));
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<ActionResult<PlannedTransactionDto>> UpdateTransaction(int id, UpdatePlannedTransactionInput transactionInput)
    {
        return Ok(await transactionService.UpdatePlannedTransaction(id, transactionInput));
    }
}
