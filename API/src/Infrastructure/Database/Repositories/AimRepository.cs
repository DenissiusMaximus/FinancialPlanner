using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class AimRepository(ApplicationDbContext context) : IAimRepository
{
    public Task<Aim?> GetByIdAsync(int id, int userId, CancellationToken ct)
        => context.Aims
            .Include(a => a.Currency)
            .Include(a => a.SourceAims).ThenInclude(sa => sa.Source).ThenInclude(s => s.Currency)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<IReadOnlyList<Aim>> GetAllAsync(int userId, CancellationToken ct)
        => await context.Aims
            .AsNoTrackingWithIdentityResolution()
            .Include(a => a.Currency)
            .Include(a => a.SourceAims).ThenInclude(sa => sa.Source).ThenInclude(s => s.Currency)
            .Where(a => a.UserId == userId)
            .ToListAsync(ct);

    public async Task<Aim> AddAsync(Aim aim, CancellationToken ct)
        => (await context.Aims.AddAsync(aim, ct)).Entity;

    public void Remove(Aim aim) => context.Aims.Remove(aim);

    public Task<SourceAim?> GetSourceLinkAsync(int aimId, int sourceId, CancellationToken ct)
        => context.Set<SourceAim>().FirstOrDefaultAsync(sa => sa.AimId == aimId && sa.SourceId == sourceId, ct);

    public void AddSourceLink(SourceAim sourceAim) => context.Set<SourceAim>().Add(sourceAim);

    public void RemoveSourceLink(SourceAim sourceAim) => context.Set<SourceAim>().Remove(sourceAim);
}
