using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Application.Features.Transactions.Queries.GetTransactions;

public sealed record GetTransactionsQuery(
    int Offset = 0,
    int Limit = 20,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    TransactionSortBy? SortBy = null,
    int? CategoryId = null,
    bool SortDescending = false);
