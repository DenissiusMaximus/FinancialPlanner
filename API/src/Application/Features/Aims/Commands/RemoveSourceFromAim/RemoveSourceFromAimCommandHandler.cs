using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Aims.Commands.RemoveSourceFromAim;

public class RemoveSourceFromAimCommandHandler(
    IAimRepository aimRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(RemoveSourceFromAimCommand command, CancellationToken ct)
    {
        var userId = currentUser.RequiredUserId;

        var aim = await aimRepository.GetByIdAsync(command.AimId, userId, ct);
        if (aim is null)
            return Result.Failure(AimErrors.NotFound(command.AimId));

        var link = await aimRepository.GetSourceLinkAsync(command.AimId, command.SourceId, ct);
        if (link is null)
            return Result.Failure(AimErrors.SourceLinkNotFound(command.AimId, command.SourceId));

        aimRepository.RemoveSourceLink(link);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
