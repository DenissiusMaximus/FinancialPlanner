using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class BlacklistedTokenRepository(ApplicationDbContext context) : IBlacklistedTokenRepository
{
    public Task<bool> ExistsAsync(string jti, CancellationToken ct)
        => context.BlacklistedTokens.AsNoTracking().AnyAsync(t => t.Jti == jti, ct);

    public async Task AddAsync(BlacklistedToken token, CancellationToken ct)
        => await context.BlacklistedTokens.AddAsync(token, ct);
}
