using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionTypeController(ITransactionTypeService transactionTypeService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionTypeDto>> GetTransactionType(int id)
    {
        return Ok(await transactionTypeService.GetTransactionType(id));
    }

    [HttpGet]
    public async Task<ActionResult<List<TransactionTypeDto>>> GetTransactions()
    {
        return Ok(await transactionTypeService.GetTransactionTypes());
    }
}