namespace FinancialPlanner.Application.Features.Frequencies.Commands.UpdateFrequency;

public sealed record UpdateFrequencyCommand(int Id, string? Name, int? IntervalUnitId, int? IntervalValue);
