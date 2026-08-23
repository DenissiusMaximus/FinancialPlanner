using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories.Filters;

namespace FinancialPlanner.Domain.Repositories;

public interface IPlannedTransactionRepository
{
    Task<PlannedTransaction?> GetByIdAsync(int id, int userId, CancellationToken ct);

    Task<(IReadOnlyList<PlannedTransaction> Items, int TotalCount)> GetPagedAsync(int userId, PlannedTransactionFilter filter, CancellationToken ct);

    Task<PlannedTransaction> AddAsync(PlannedTransaction plannedTransaction, CancellationToken ct);

    void Remove(PlannedTransaction plannedTransaction);
}
