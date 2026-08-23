using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Repositories.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace FinancialPlanner.Infrastructure.Database.Repositories;

public class TransactionRepository(ApplicationDbContext context) : ITransactionRepository
{
    private static readonly Func<IQueryable<Transaction>, IIncludableQueryable<Transaction, object?>> IncludeGraph = q => q
        .Include(t => t.Category)
        .Include(t => t.Currency)
        .Include(t => t.Source).ThenInclude(s => s.Currency)
        .Include(t => t.DestinationSource).ThenInclude(s => s!.Currency)
        .Include(t => t.TransactionType);

    public Task<Transaction?> GetByIdAsync(int id, int userId, CancellationToken ct)
        => IncludeGraph(context.Transactions)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(int userId, TransactionFilter filter, CancellationToken ct)
    {
        var query = context.Transactions.AsNoTracking().Where(t => t.UserId == userId);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.Category != null && t.Category.Id == filter.CategoryId);

        if (filter.FromDate.HasValue)
        {
            var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.Date >= from);
        }

        if (filter.ToDate.HasValue)
        {
            var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(t => t.Date <= to);
        }

        var totalCount = await query.CountAsync(ct);

        query = filter.SortBy switch
        {
            TransactionSortBy.Date => filter.SortDescending ? query.OrderByDescending(t => t.Date) : query.OrderBy(t => t.Date),
            TransactionSortBy.Amount => filter.SortDescending ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
            _ => query
        };

        var items = await IncludeGraph(query)
            .Skip(filter.Offset)
            .Take(filter.Limit)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken ct)
        => (await context.Transactions.AddAsync(transaction, ct)).Entity;

    public void Remove(Transaction transaction) => context.Transactions.Remove(transaction);
}
