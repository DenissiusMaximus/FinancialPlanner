using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface ITransactionTypeRepository
{
    Task<TransactionType?> GetByIdAsync(int id, CancellationToken ct);

    Task<IReadOnlyList<TransactionType>> GetAllAsync(CancellationToken ct);
}
