using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class SourceErrors
{
    public static Error NotFound(int id) => new(
        "Sources.NotFound",
        $"Source with id '{id}' was not found.",
        ErrorType.NotFound);

    public static Error DestinationNotFound(int id) => new(
        "Sources.DestinationNotFound",
        $"Destination source with id '{id}' was not found.",
        ErrorType.NotFound);
}
