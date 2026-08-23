namespace FinancialPlanner.Application.Features.Sources.Commands.CreateSource;

public sealed record CreateSourceCommand(string Name, decimal Amount, int CurrencyId);
