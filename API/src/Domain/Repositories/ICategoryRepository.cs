using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, int userId, CancellationToken ct);

    Task<IReadOnlyList<Category>> GetAllAsync(int userId, CancellationToken ct);

    Task<Category> AddAsync(Category category, CancellationToken ct);

    void Remove(Category category);
}
