using API.Models;

namespace API.Services.InteralUnit;

public interface IIntervalUnitService
{
    Task<List<IntervalUnitDto>> GetIntervalUnits();
    Task<IntervalUnitDto?> GetIntervalUnitById(int id);
}