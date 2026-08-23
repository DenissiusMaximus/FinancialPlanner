namespace FinancialPlanner.Application.Features.Transactions.Commands.CreateTransaction;

public sealed record CreateTransactionCommand(
    decimal Amount,
    string? Comment,
    DateTime Date,
    int? CategoryId,
    int SourceId,
    int? DestinationSourceId,
    int CurrencyId,
    int TransactionTypeId);
