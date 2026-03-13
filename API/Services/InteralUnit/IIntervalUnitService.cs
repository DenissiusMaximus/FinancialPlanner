using API.Models;

namespace API.Services.InteralUnit;

public interface IIntervalUnitService
{
    Task<IReadOnlyCollection<IntervalUnitDto>> GetIntervalUnits();
    Task<IntervalUnitDto?> GetIntervalUnitById(int id);
}