namespace FinancialPlanner.Application.Features.PlannedTransactions.Commands.UpdatePlannedTransaction;

public sealed record UpdatePlannedTransactionCommand(
    int Id,
    string? Name,
    decimal? Amount,
    DateTime? StartDate,
    int? CurrencyId,
    int? TransactionTypeId,
    int? CategoryId,
    int? SourceId,
    int? FrequencyId);
