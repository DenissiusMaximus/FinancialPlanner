using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Domain.Errors;

namespace FinancialPlanner.Domain.Services;

public sealed class BalanceManager : IBalanceManager
{
    public bool IsBalanceAffected(Transaction original, Transaction updated)
    {
        return original.Amount != updated.Amount
            || original.TransactionTypeId != updated.TransactionTypeId
            || original.SourceId != updated.SourceId
            || original.DestinationSourceId != updated.DestinationSourceId;
    }

    public Result Apply(Transaction transaction, Source source, Source? destinationSource)
    {
        switch ((TransactionTypeEnum)transaction.TransactionTypeId)
        {
            case TransactionTypeEnum.Adjustment:
                source.Amount = transaction.Amount;
                return Result.Success();

            case TransactionTypeEnum.Expense:
                source.Amount -= transaction.Amount;
                return Result.Success();

            case TransactionTypeEnum.Income:
                source.Amount += transaction.Amount;
                return Result.Success();

            case TransactionTypeEnum.Transfer:
                if (destinationSource is null)
                    return Result.Failure(SourceErrors.DestinationNotFound(transaction.DestinationSourceId ?? 0));

                source.Amount -= transaction.Amount;
                destinationSource.Amount += transaction.Amount;
                return Result.Success();

            default:
                return Result.Failure(TransactionErrors.UnknownTransactionType(transaction.TransactionTypeId));
        }
    }

    public Result Revert(Transaction transaction, Source source, Source? destinationSource)
    {
        switch ((TransactionTypeEnum)transaction.TransactionTypeId)
        {
            case TransactionTypeEnum.Transfer:
                if (destinationSource is null)
                    return Result.Failure(SourceErrors.DestinationNotFound(transaction.DestinationSourceId ?? 0));

                destinationSource.Amount -= transaction.Amount;
                source.Amount += transaction.Amount;
                return Result.Success();

            case TransactionTypeEnum.Expense:
                source.Amount += transaction.Amount;
                return Result.Success();

            case TransactionTypeEnum.Income:
                source.Amount -= transaction.Amount;
                return Result.Success();

            case TransactionTypeEnum.Adjustment:
                return Result.Failure(TransactionErrors.AdjustmentNotReversible);

            default:
                return Result.Failure(TransactionErrors.UnknownTransactionType(transaction.TransactionTypeId));
        }
    }
}
