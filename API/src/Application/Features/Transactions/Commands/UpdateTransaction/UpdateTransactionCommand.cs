namespace FinancialPlanner.Application.Features.Transactions.Commands.UpdateTransaction;

public sealed record UpdateTransactionCommand(
    int Id,
    decimal? Amount,
    string? Comment,
    DateOnly? Date,
    int? CategoryId,
    int? SourceId,
    int? DestinationSourceId,
    int? CurrencyId,
    int? TransactionTypeId);
