using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Application.Features.Aims.Queries.GetAims;

public sealed record GetAimsQuery(
    int Limit = 10,
    int Offset = 0,
    List<int>? SourceIds = null,
    bool ClosedOnly = false,
    bool SortDescending = true,
    AimSortBy SortBy = AimSortBy.Amount);
