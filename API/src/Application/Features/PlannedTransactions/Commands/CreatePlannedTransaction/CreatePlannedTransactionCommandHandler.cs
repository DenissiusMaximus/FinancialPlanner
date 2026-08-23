using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.PlannedTransactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using Mapster;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Commands.CreatePlannedTransaction;

public class CreatePlannedTransactionCommandHandler(
    IValidator<CreatePlannedTransactionCommand> validator,
    IPlannedTransactionRepository plannedTransactionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<PlannedTransactionDto>> HandleAsync(CreatePlannedTransactionCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<PlannedTransactionDto>(validationResult.ToValidationError());

        var plannedTransaction = command.Adapt<PlannedTransaction>();
        plannedTransaction.UserId = currentUser.RequiredUserId;

        var added = await plannedTransactionRepository.AddAsync(plannedTransaction, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<PlannedTransactionDto>(added);
    }
}
