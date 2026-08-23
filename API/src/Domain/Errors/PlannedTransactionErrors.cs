using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class PlannedTransactionErrors
{
    public static Error NotFound(int id) => new(
        "PlannedTransactions.NotFound",
        $"Planned transaction with id '{id}' was not found.",
        ErrorType.NotFound);
}
