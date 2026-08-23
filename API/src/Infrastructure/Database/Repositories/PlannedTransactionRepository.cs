using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Repositories.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class PlannedTransactionRepository(ApplicationDbContext context) : IPlannedTransactionRepository
{
    private static readonly Func<IQueryable<PlannedTransaction>, IIncludableQueryable<PlannedTransaction, object?>> IncludeGraph = q => q
        .Include(t => t.Category)
        .Include(t => t.Currency)
        .Include(t => t.Frequency).ThenInclude(f => f.IntervalUnitNavigation)
        .Include(t => t.Source).ThenInclude(s => s.Currency)
        .Include(t => t.TransactionType);

    public Task<PlannedTransaction?> GetByIdAsync(int id, int userId, CancellationToken ct)
        => IncludeGraph(context.PlannedTransactions)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

    public async Task<(IReadOnlyList<PlannedTransaction> Items, int TotalCount)> GetPagedAsync(int userId, PlannedTransactionFilter filter, CancellationToken ct)
    {
        var query = context.PlannedTransactions.AsNoTracking().Where(t => t.UserId == userId);

        if (filter.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

        var totalCount = await query.CountAsync(ct);

        query = filter.SortDescending
            ? query.OrderByDescending(t => t.Amount)
            : query.OrderBy(t => t.Amount);

        var items = await IncludeGraph(query)
            .Skip(filter.Offset)
            .Take(filter.Limit)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<PlannedTransaction> AddAsync(PlannedTransaction plannedTransaction, CancellationToken ct)
        => (await context.PlannedTransactions.AddAsync(plannedTransaction, ct)).Entity;

    public void Remove(PlannedTransaction plannedTransaction) => context.PlannedTransactions.Remove(plannedTransaction);
}
