using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Application.Features.Aims.Queries.GetAims;

internal static class AimDtoQueryExtensions
{
    public static IEnumerable<AimDto> FilterBySources(this IEnumerable<AimDto> aims, List<int>? sourceIds)
    {
        if (sourceIds is null || sourceIds.Count == 0)
            return aims;

        return aims.Where(a => a.Sources?.Any(s => sourceIds.Contains(s.Id)) == true);
    }

    public static IEnumerable<AimDto> FilterByClosed(this IEnumerable<AimDto> aims, bool closedOnly)
    {
        return closedOnly ? aims.Where(a => a.IsClosed) : aims;
    }

    public static IEnumerable<AimDto> ApplySorting(this IEnumerable<AimDto> aims, AimSortBy? sortBy, bool descending)
    {
        return sortBy switch
        {
            AimSortBy.Amount => descending ? aims.OrderByDescending(a => a.Amount) : aims.OrderBy(a => a.Amount),
            AimSortBy.Priority => descending ? aims.OrderByDescending(a => a.Priority) : aims.OrderBy(a => a.Priority),
            _ => aims
        };
    }
}
