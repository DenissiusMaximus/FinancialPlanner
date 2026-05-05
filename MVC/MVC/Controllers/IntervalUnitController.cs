using API.Models;
using API.Services.InteralUnit;
using Microsoft.AspNetCore.Mvc;

namespace API.Dtos;

[ApiController]
[Route("api/[controller]")]
public class IntervalUnitController(IIntervalUnitService intervalUnitService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IntervalUnitDto>>> Get()
    {
        return Ok(await intervalUnitService.GetIntervalUnits());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IntervalUnitDto>> Get(int id)
    {
        return Ok(await intervalUnitService.GetIntervalUnitById(id));
    }
}