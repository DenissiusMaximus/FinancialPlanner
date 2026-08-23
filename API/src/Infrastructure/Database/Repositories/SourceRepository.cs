using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class SourceRepository(ApplicationDbContext context) : ISourceRepository
{
    public Task<Source?> GetByIdAsync(int id, int userId, CancellationToken ct)
        => context.Sources
            .Include(s => s.Currency)
            .Include(s => s.SourceAims).ThenInclude(sa => sa.Aim).ThenInclude(a => a.Currency)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);

    public async Task<IReadOnlyList<Source>> GetAllAsync(int userId, CancellationToken ct)
        => await context.Sources
            .AsNoTracking()
            .Include(s => s.Currency)
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

    public async Task<Source> AddAsync(Source source, CancellationToken ct)
        => (await context.Sources.AddAsync(source, ct)).Entity;

    public void Remove(Source source) => context.Sources.Remove(source);
}
