namespace FinancialPlanner.Domain.Repositories.Filters;

public sealed record PlannedTransactionFilter(
    decimal? MinAmount,
    decimal? MaxAmount,
    bool SortDescending,
    int Offset,
    int Limit);
