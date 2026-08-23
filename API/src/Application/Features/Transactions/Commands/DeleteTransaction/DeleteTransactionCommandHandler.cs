using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Services;

namespace FinancialPlanner.Application.Features.Transactions.Commands.DeleteTransaction;

public class DeleteTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    ISourceRepository sourceRepository,
    IUnitOfWork unitOfWork,
    IBalanceManager balanceManager,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(DeleteTransactionCommand command, CancellationToken ct)
    {
        var userId = currentUser.RequiredUserId;

        var transaction = await transactionRepository.GetByIdAsync(command.Id, userId, ct);
        if (transaction is null)
            return Result.Failure(TransactionErrors.NotFound(command.Id));

        if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Adjustment)
            return Result.Failure(TransactionErrors.AdjustmentNotDeletable);

        var source = await sourceRepository.GetByIdAsync(transaction.SourceId, userId, ct);
        if (source is null)
            return Result.Failure(SourceErrors.NotFound(transaction.SourceId));

        Source? destinationSource = null;
        if (transaction.DestinationSourceId is { } destinationId)
        {
            destinationSource = await sourceRepository.GetByIdAsync(destinationId, userId, ct);
            if (destinationSource is null)
                return Result.Failure(SourceErrors.DestinationNotFound(destinationId));
        }

        await using var dbTransaction = await unitOfWork.BeginTransactionAsync(ct);

        var revertResult = balanceManager.Revert(transaction, source, destinationSource);
        if (revertResult.IsFailure)
            return Result.Failure(revertResult.Error);

        transactionRepository.Remove(transaction);
        await unitOfWork.SaveChangesAsync(ct);
        await dbTransaction.CommitAsync(ct);

        return Result.Success();
    }
}
