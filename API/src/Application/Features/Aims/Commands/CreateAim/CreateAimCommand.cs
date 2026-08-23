namespace FinancialPlanner.Application.Features.Aims.Commands.CreateAim;

public sealed record CreateAimCommand(string Name, decimal Amount, int Priority, int? CurrencyId);
