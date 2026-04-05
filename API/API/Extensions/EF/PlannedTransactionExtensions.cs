using API.Models;

namespace API.Extensions;

public static class PlannedTransactionExtensions
{
    public static IQueryable<PlannedTransaction> FilterByAmount(this IQueryable<PlannedTransaction> query, decimal? minimalAmount, decimal? maximalAmount)
    {
        if (minimalAmount.HasValue)
            query = query.Where(a => a.Amount >= minimalAmount.Value);

        if (maximalAmount.HasValue)
            query = query.Where(a => a.Amount <= maximalAmount.Value);

        return query;
    }

    public static IQueryable<PlannedTransaction> ApplySorting(this IQueryable<PlannedTransaction> query, bool descending)
    {
        return descending
            ? query.OrderByDescending(a => a.Amount)
            : query.OrderBy(a => a.Amount);
    }
}