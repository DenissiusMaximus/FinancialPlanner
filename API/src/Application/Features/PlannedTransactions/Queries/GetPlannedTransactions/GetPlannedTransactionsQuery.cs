namespace FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactions;

public sealed record GetPlannedTransactionsQuery(
    int Offset = 0,
    int Limit = 20,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    bool SortDescending = true);
