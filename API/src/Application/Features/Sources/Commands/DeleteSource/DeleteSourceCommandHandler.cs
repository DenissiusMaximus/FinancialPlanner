using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Sources.Commands.DeleteSource;

public class DeleteSourceCommandHandler(
    ISourceRepository sourceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(DeleteSourceCommand command, CancellationToken ct)
    {
        var source = await sourceRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (source is null)
            return Result.Failure(SourceErrors.NotFound(command.Id));

        sourceRepository.Remove(source);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
