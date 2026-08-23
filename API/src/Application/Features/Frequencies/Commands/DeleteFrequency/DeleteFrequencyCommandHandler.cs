using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Frequencies.Commands.DeleteFrequency;

public class DeleteFrequencyCommandHandler(
    IFrequencyRepository frequencyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(DeleteFrequencyCommand command, CancellationToken ct)
    {
        var frequency = await frequencyRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (frequency is null)
            return Result.Failure(FrequencyErrors.NotFound(command.Id));

        frequencyRepository.Remove(frequency);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
