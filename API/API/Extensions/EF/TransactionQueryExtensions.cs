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
        {
            var minTime = TimeOnly.MinValue;
            var fromDateTime = fromDate.Value.ToDateTime(minTime);
            query = query.Where(t => t.Date >= fromDateTime);
        }

        if (toDate != null)
        {
            var maxTime = TimeOnly.MaxValue;
            var toDateTime = toDate.Value.ToDateTime(maxTime);
            query = query.Where(t => t.Date <= toDateTime);
        }

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
