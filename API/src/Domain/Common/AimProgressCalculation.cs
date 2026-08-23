namespace FinancialPlanner.Domain.Common;

public sealed class AimProgressCalculation
{
    public required IReadOnlyDictionary<int, AimProgress> ProgressByAimId { get; init; }

    public required IReadOnlyDictionary<int, decimal> RemainingAmountBySourceId { get; init; }
}
