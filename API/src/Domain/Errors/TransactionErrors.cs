using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class TransactionErrors
{
    public static Error NotFound(int id) => new(
        "Transactions.NotFound",
        $"Transaction with id '{id}' was not found.",
        ErrorType.NotFound);

    public static readonly Error AdjustmentNotUpdatable = new(
        "Transactions.AdjustmentNotUpdatable",
        "Adjustment transactions cannot be updated.",
        ErrorType.Validation);

    public static readonly Error AdjustmentNotDeletable = new(
        "Transactions.AdjustmentNotDeletable",
        "Adjustment transactions cannot be deleted.",
        ErrorType.Validation);

    public static readonly Error AdjustmentNotReversible = new(
        "Transactions.AdjustmentNotReversible",
        "Adjustment transactions cannot be reversed.",
        ErrorType.Conflict);

    public static Error UnknownTransactionType(int transactionTypeId) => new(
        "Transactions.UnknownTransactionType",
        $"Transaction type with id '{transactionTypeId}' is not recognized.",
        ErrorType.Validation);
}
