using API.Inputs;
using API.Models;
using API.Services.Aim;

namespace API.Extensions;

public static class AimQueryExtensions
{
    public static IQueryable<Aim> FilterBySources(this IQueryable<Aim> query, List<int>? sourceIds)
    {
        if (sourceIds == null || sourceIds.Count == 0)
            return query;

        return query.Where(a => a.SourceAims.Any(sa => sourceIds.Contains(sa.SourceId)));
    }

    public static IQueryable<Aim> FilterByClosed(this IQueryable<Aim> query, bool closedOnly)
    {
        if (!closedOnly)
            return query;

        return query.Where(a => a.IsClosed == true);
    }

    public static IQueryable<Aim> ApplySorting(this IQueryable<Aim> query, AimSortBy? sortBy, bool descending)
    {
        if (!sortBy.HasValue) return query;

        return sortBy.Value switch
        {
            AimSortBy.Amount => descending
                ? query.OrderByDescending(a => a.Amount)
                : query.OrderBy(a => a.Amount),
            
            AimSortBy.Priority => descending
                ? query.OrderByDescending(a => a.Priority)
                : query.OrderBy(a => a.Priority),

            _ => query
        };
    }
}