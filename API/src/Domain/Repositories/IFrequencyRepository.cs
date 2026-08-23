using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface IFrequencyRepository
{
    Task<Frequency?> GetByIdAsync(int id, int userId, CancellationToken ct);

    Task<IReadOnlyList<Frequency>> GetAllAsync(int userId, CancellationToken ct);

    Task<IReadOnlyList<Frequency>> GetUserOwnedAsync(int userId, CancellationToken ct);

    Task<Frequency> AddAsync(Frequency frequency, CancellationToken ct);

    void Remove(Frequency frequency);
}
