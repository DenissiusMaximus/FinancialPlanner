using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(int id, CancellationToken ct);

    Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct);
}
