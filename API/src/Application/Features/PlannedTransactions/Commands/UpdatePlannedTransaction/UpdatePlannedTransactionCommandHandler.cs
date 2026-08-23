using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.PlannedTransactions.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Commands.UpdatePlannedTransaction;

public class UpdatePlannedTransactionCommandHandler(
    IValidator<UpdatePlannedTransactionCommand> validator,
    IPlannedTransactionRepository plannedTransactionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IPatchMapper patchMapper,
    IMapper mapper)
{
    public async Task<Result<PlannedTransactionDto>> HandleAsync(UpdatePlannedTransactionCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<PlannedTransactionDto>(validationResult.ToValidationError());

        var plannedTransaction = await plannedTransactionRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (plannedTransaction is null)
            return Result.Failure<PlannedTransactionDto>(PlannedTransactionErrors.NotFound(command.Id));

        patchMapper.PatchInto(command, plannedTransaction);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<PlannedTransactionDto>(plannedTransaction);
    }
}
