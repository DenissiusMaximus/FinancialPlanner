using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class IntervalUnitRepository(ApplicationDbContext context) : IIntervalUnitRepository
{
    public Task<IntervalUnit?> GetByIdAsync(int id, CancellationToken ct)
        => context.IntervalUnits.AsNoTracking().FirstOrDefaultAsync(iu => iu.Id == id, ct);

    public async Task<IReadOnlyList<IntervalUnit>> GetAllAsync(CancellationToken ct)
        => await context.IntervalUnits.AsNoTracking().ToListAsync(ct);
}
