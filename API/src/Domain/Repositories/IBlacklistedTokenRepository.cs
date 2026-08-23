using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface IBlacklistedTokenRepository
{
    Task<bool> ExistsAsync(string jti, CancellationToken ct);

    Task AddAsync(BlacklistedToken token, CancellationToken ct);
}
