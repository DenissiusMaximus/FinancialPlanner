using API.Inputs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController(ITransactionService transactionService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<TransactionDto>>> GetUsersTransactions([FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        return Ok(await transactionService.GetUsersTransactions(limit, offset));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> GetTransactionById(int id)
    {
        return Ok(await transactionService.GetTransactionById(id));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(CreateTransactionInput transactionInput)
    {
        return Ok(await transactionService.CreateTransaction(transactionInput));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteTransaction(int id)
    {
        return Ok(await transactionService.DeleteTransaction(id));
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(int id, UpdateTransactionInput transactionInput)
    {
        return Ok(await transactionService.UpdateTransaction(id, transactionInput));
    }
}