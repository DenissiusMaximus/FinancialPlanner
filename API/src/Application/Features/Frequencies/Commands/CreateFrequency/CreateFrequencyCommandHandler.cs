using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using Mapster;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Frequencies.Commands.CreateFrequency;

public class CreateFrequencyCommandHandler(
    IValidator<CreateFrequencyCommand> validator,
    IFrequencyRepository frequencyRepository,
    IIntervalUnitRepository intervalUnitRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<FrequencyDto>> HandleAsync(CreateFrequencyCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<FrequencyDto>(validationResult.ToValidationError());

        var intervalUnit = await intervalUnitRepository.GetByIdAsync(command.IntervalUnitId, ct);
        if (intervalUnit is null)
            return Result.Failure<FrequencyDto>(FrequencyErrors.IntervalUnitNotFound(command.IntervalUnitId));

        var frequency = command.Adapt<Frequency>();
        frequency.UserId = currentUser.RequiredUserId;

        var added = await frequencyRepository.AddAsync(frequency, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var dto = mapper.Map<FrequencyDto>(added);
        dto.IntervalUnit = mapper.Map<IntervalUnitDto>(intervalUnit);

        return dto;
    }
}
