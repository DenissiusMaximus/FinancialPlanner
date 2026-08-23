using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class FrequencyRepository(ApplicationDbContext context) : IFrequencyRepository
{
    public Task<Frequency?> GetByIdAsync(int id, int userId, CancellationToken ct)
        => context.Frequencies
            .Include(f => f.IntervalUnitNavigation)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId, ct);

    public async Task<IReadOnlyList<Frequency>> GetAllAsync(int userId, CancellationToken ct)
        => await context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .Where(f => f.UserId == userId || f.UserId == null)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Frequency>> GetUserOwnedAsync(int userId, CancellationToken ct)
        => await context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .Where(f => f.UserId == userId)
            .ToListAsync(ct);

    public async Task<Frequency> AddAsync(Frequency frequency, CancellationToken ct)
        => (await context.Frequencies.AddAsync(frequency, ct)).Entity;

    public void Remove(Frequency frequency) => context.Frequencies.Remove(frequency);
}
