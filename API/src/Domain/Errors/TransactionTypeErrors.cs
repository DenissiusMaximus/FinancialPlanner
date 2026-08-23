using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class TransactionTypeErrors
{
    public static Error NotFound(int id) => new(
        "TransactionTypes.NotFound",
        $"Transaction type with id '{id}' was not found.",
        ErrorType.NotFound);
}
