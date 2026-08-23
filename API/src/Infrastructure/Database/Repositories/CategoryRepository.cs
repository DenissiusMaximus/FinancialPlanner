using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(int id, int userId, CancellationToken ct)
        => context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public async Task<IReadOnlyList<Category>> GetAllAsync(int userId, CancellationToken ct)
        => await context.Categories.AsNoTracking().Where(c => c.UserId == userId).ToListAsync(ct);

    public async Task<Category> AddAsync(Category category, CancellationToken ct)
        => (await context.Categories.AddAsync(category, ct)).Entity;

    public void Remove(Category category) => context.Categories.Remove(category);
}
