using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Transactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Services;
using FluentValidation;
using Mapster;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler(
    IValidator<CreateTransactionCommand> validator,
    ITransactionRepository transactionRepository,
    ISourceRepository sourceRepository,
    IUnitOfWork unitOfWork,
    IBalanceManager balanceManager,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<TransactionDto>> HandleAsync(CreateTransactionCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<TransactionDto>(validationResult.ToValidationError());

        var userId = currentUser.RequiredUserId;

        var source = await sourceRepository.GetByIdAsync(command.SourceId, userId, ct);
        if (source is null)
            return Result.Failure<TransactionDto>(SourceErrors.NotFound(command.SourceId));

        Source? destinationSource = null;
        if (command.DestinationSourceId is { } destinationId && destinationId is not 0 and not -1)
        {
            destinationSource = await sourceRepository.GetByIdAsync(destinationId, userId, ct);
            if (destinationSource is null)
                return Result.Failure<TransactionDto>(SourceErrors.DestinationNotFound(destinationId));
        }

        var transaction = command.Adapt<Transaction>();
        transaction.UserId = userId;

        await using var dbTransaction = await unitOfWork.BeginTransactionAsync(ct);

        var added = await transactionRepository.AddAsync(transaction, ct);

        var applyResult = balanceManager.Apply(transaction, source, destinationSource);
        if (applyResult.IsFailure)
            return Result.Failure<TransactionDto>(applyResult.Error);

        await unitOfWork.SaveChangesAsync(ct);
        await dbTransaction.CommitAsync(ct);

        return mapper.Map<TransactionDto>(added);
    }
}
