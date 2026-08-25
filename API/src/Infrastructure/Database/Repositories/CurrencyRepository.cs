using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class CurrencyRepository(ApplicationDbContext context) : ICurrencyRepository
{
    public Task<Currency?> GetByIdAsync(int id, CancellationToken ct)
        => context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct)
        => await context.Currencies.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Currency>> GetAllTrackedAsync(CancellationToken ct)
        => await context.Currencies.ToListAsync(ct);
}
