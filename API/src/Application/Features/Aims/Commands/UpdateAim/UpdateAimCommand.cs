namespace FinancialPlanner.Application.Features.Aims.Commands.UpdateAim;

public sealed record UpdateAimCommand(int Id, string? Name, decimal? Amount, int? Priority, int? CurrencyId);
