namespace FinancialPlanner.Application.Features.PlannedTransactions.Commands.CreatePlannedTransaction;

public sealed record CreatePlannedTransactionCommand(
    string Name,
    decimal Amount,
    DateTime StartDate,
    int CurrencyId,
    int TransactionTypeId,
    int? CategoryId,
    int SourceId,
    int FrequencyId);
