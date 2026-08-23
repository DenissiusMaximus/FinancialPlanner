using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Frequencies.Commands.UpdateFrequency;

public class UpdateFrequencyCommandHandler(
    IValidator<UpdateFrequencyCommand> validator,
    IFrequencyRepository frequencyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IPatchMapper patchMapper,
    IMapper mapper)
{
    public async Task<Result<FrequencyDto>> HandleAsync(UpdateFrequencyCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<FrequencyDto>(validationResult.ToValidationError());

        var frequency = await frequencyRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (frequency is null)
            return Result.Failure<FrequencyDto>(FrequencyErrors.NotFound(command.Id));

        patchMapper.PatchInto(command, frequency);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<FrequencyDto>(frequency);
    }
}
