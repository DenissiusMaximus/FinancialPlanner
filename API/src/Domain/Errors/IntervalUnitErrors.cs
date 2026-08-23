using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class IntervalUnitErrors
{
    public static Error NotFound(int id) => new(
        "IntervalUnits.NotFound",
        $"Interval unit with id '{id}' was not found.",
        ErrorType.NotFound);
}
