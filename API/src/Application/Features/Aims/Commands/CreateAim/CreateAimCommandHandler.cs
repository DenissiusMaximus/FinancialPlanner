using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using Mapster;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Aims.Commands.CreateAim;

public class CreateAimCommandHandler(
    IValidator<CreateAimCommand> validator,
    IAimRepository aimRepository,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<AimDto>> HandleAsync(CreateAimCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<AimDto>(validationResult.ToValidationError());

        if (command.CurrencyId is { } currencyId)
        {
            var currency = await currencyRepository.GetByIdAsync(currencyId, ct);
            if (currency is null)
                return Result.Failure<AimDto>(CurrencyErrors.NotFound(currencyId));
        }

        var aim = command.Adapt<Aim>();
        aim.UserId = currentUser.RequiredUserId;

        var added = await aimRepository.AddAsync(aim, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<AimDto>(added);
    }
}
