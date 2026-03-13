using API.Models;
using API.Services.Currency;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CurrencyController(ICurrencyService currencyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CurrencyDto>>> GetAllCurrencies()
    {
        return Ok(await currencyService.GetAllCurrencies());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CurrencyDto>> GetCurrencyById(int id)
    {
        return Ok(await currencyService.GetCurrencyById(id));
    }
}