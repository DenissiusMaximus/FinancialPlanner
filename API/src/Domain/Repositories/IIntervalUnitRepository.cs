using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface IIntervalUnitRepository
{
    Task<IntervalUnit?> GetByIdAsync(int id, CancellationToken ct);

    Task<IReadOnlyList<IntervalUnit>> GetAllAsync(CancellationToken ct);
}
