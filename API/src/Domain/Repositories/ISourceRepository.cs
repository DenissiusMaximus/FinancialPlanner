using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface ISourceRepository
{
    Task<Source?> GetByIdAsync(int id, int userId, CancellationToken ct);

    Task<IReadOnlyList<Source>> GetAllAsync(int userId, CancellationToken ct);

    Task<Source> AddAsync(Source source, CancellationToken ct);

    void Remove(Source source);
}
