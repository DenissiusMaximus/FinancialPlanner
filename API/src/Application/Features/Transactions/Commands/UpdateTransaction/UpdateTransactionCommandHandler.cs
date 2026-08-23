using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Transactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Services;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommandHandler(
    IValidator<UpdateTransactionCommand> validator,
    ITransactionRepository transactionRepository,
    ISourceRepository sourceRepository,
    IUnitOfWork unitOfWork,
    IBalanceManager balanceManager,
    ICurrentUserContext currentUser,
    IPatchMapper patchMapper,
    IMapper mapper)
{
    public async Task<Result<TransactionDto>> HandleAsync(UpdateTransactionCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<TransactionDto>(validationResult.ToValidationError());

        var userId = currentUser.RequiredUserId;

        var transaction = await transactionRepository.GetByIdAsync(command.Id, userId, ct);
        if (transaction is null)
            return Result.Failure<TransactionDto>(TransactionErrors.NotFound(command.Id));

        if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Adjustment)
            return Result.Failure<TransactionDto>(TransactionErrors.AdjustmentNotUpdatable);

        var original = new Transaction
        {
            Amount = transaction.Amount,
            TransactionTypeId = transaction.TransactionTypeId,
            SourceId = transaction.SourceId,
            DestinationSourceId = transaction.DestinationSourceId
        };

        patchMapper.PatchInto(command, transaction);

        var balanceAffected = balanceManager.IsBalanceAffected(original, transaction);

        await using var dbTransaction = await unitOfWork.BeginTransactionAsync(ct);

        if (balanceAffected)
        {
            var originalSource = await sourceRepository.GetByIdAsync(original.SourceId, userId, ct);
            if (originalSource is null)
                return Result.Failure<TransactionDto>(SourceErrors.NotFound(original.SourceId));

            Source? originalDestinationSource = null;
            if (original.DestinationSourceId is { } originalDestinationId)
            {
                originalDestinationSource = await sourceRepository.GetByIdAsync(originalDestinationId, userId, ct);
                if (originalDestinationSource is null)
                    return Result.Failure<TransactionDto>(SourceErrors.DestinationNotFound(originalDestinationId));
            }

            var revertResult = balanceManager.Revert(original, originalSource, originalDestinationSource);
            if (revertResult.IsFailure)
                return Result.Failure<TransactionDto>(revertResult.Error);

            var newSource = transaction.SourceId == original.SourceId
                ? originalSource
                : await sourceRepository.GetByIdAsync(transaction.SourceId, userId, ct);
            if (newSource is null)
                return Result.Failure<TransactionDto>(SourceErrors.NotFound(transaction.SourceId));

            Source? newDestinationSource = null;
            if (transaction.DestinationSourceId is { } newDestinationId)
            {
                newDestinationSource = newDestinationId switch
                {
                    _ when newDestinationId == original.DestinationSourceId => originalDestinationSource,
                    _ when newDestinationId == newSource.Id => newSource,
                    _ => await sourceRepository.GetByIdAsync(newDestinationId, userId, ct)
                };

                if (newDestinationSource is null)
                    return Result.Failure<TransactionDto>(SourceErrors.DestinationNotFound(newDestinationId));
            }

            var applyResult = balanceManager.Apply(transaction, newSource, newDestinationSource);
            if (applyResult.IsFailure)
                return Result.Failure<TransactionDto>(applyResult.Error);
        }

        await unitOfWork.SaveChangesAsync(ct);
        await dbTransaction.CommitAsync(ct);

        return mapper.Map<TransactionDto>(transaction);
    }
}
