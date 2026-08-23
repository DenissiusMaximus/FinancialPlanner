using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class TransactionTypeRepository(ApplicationDbContext context) : ITransactionTypeRepository
{
    public Task<TransactionType?> GetByIdAsync(int id, CancellationToken ct)
        => context.TransactionTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<TransactionType>> GetAllAsync(CancellationToken ct)
        => await context.TransactionTypes.AsNoTracking().ToListAsync(ct);
}
