using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Domain.Repositories.Filters;

public sealed record TransactionFilter(
    int? CategoryId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    TransactionSortBy? SortBy,
    bool SortDescending,
    int Offset,
    int Limit);
