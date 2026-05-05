using API.Models;
using API.Services.Transaction;

namespace API.Extensions;

public static class TransactionQueryExtensions
{
    public static IQueryable<Transaction> FilterByCategory(this IQueryable<Transaction> query, int? categoryId)
    {
        if (categoryId == null)
            return query;

        return query.Where(t => t.Category != null && t.Category.Id == categoryId);
    }

    public static IQueryable<Transaction> FilterByDateRange(this IQueryable<Transaction> query,
        DateOnly? fromDate, DateOnly? toDate)
    {
        if (fromDate != null)
            query = query.Where(t => t.Date >= fromDate);

        if (toDate != null)
            query = query.Where(t => t.Date <= toDate);

        return query;
    }

    public static IQueryable<Transaction> ApplySorting(this IQueryable<Transaction> query,
        TransactionSortBy? sortBy, bool sortDescending)
    {
        if (!sortBy.HasValue) return query;

        return sortBy.Value switch
        {
            TransactionSortBy.Date => sortDescending
                ? query.OrderByDescending(t => t.Date)
                : query.OrderBy(t => t.Date),

            TransactionSortBy.Amount => sortDescending
                ? query.OrderByDescending(t => t.Amount)
                : query.OrderBy(t => t.Amount),

            _ => query
        };
    }
}