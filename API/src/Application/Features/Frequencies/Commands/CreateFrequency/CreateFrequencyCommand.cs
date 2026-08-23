namespace FinancialPlanner.Application.Features.Frequencies.Commands.CreateFrequency;

public sealed record CreateFrequencyCommand(string Name, int IntervalUnitId, int IntervalValue);
