using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Aims.Commands.DeleteAim;

public class DeleteAimCommandHandler(
    IAimRepository aimRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(DeleteAimCommand command, CancellationToken ct)
    {
        var aim = await aimRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (aim is null)
            return Result.Failure(AimErrors.NotFound(command.Id));

        aimRepository.Remove(aim);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
