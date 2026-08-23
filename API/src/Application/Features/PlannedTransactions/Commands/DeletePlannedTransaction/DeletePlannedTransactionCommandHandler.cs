using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Commands.DeletePlannedTransaction;

public class DeletePlannedTransactionCommandHandler(
    IPlannedTransactionRepository plannedTransactionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(DeletePlannedTransactionCommand command, CancellationToken ct)
    {
        var plannedTransaction = await plannedTransactionRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (plannedTransaction is null)
            return Result.Failure(PlannedTransactionErrors.NotFound(command.Id));

        plannedTransactionRepository.Remove(plannedTransaction);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
