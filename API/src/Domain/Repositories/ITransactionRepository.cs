using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories.Filters;

namespace FinancialPlanner.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id, int userId, CancellationToken ct);

    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(int userId, TransactionFilter filter, CancellationToken ct);

    Task<Transaction> AddAsync(Transaction transaction, CancellationToken ct);

    void Remove(Transaction transaction);
}
