using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class CurrencyErrors
{
    public static Error NotFound(int id) => new(
        "Currencies.NotFound",
        $"Currency with id '{id}' was not found.",
        ErrorType.NotFound);
}
