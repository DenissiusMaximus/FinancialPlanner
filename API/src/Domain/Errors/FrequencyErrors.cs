using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class FrequencyErrors
{
    public static Error NotFound(int id) => new(
        "Frequencies.NotFound",
        $"Frequency with id '{id}' was not found.",
        ErrorType.NotFound);

    public static Error IntervalUnitNotFound(int intervalUnitId) => new(
        "Frequencies.IntervalUnitNotFound",
        $"Interval unit with id '{intervalUnitId}' was not found.",
        ErrorType.NotFound);
}
