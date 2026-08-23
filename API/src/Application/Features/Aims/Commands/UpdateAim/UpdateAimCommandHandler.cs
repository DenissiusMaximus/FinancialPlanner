using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Aims.Commands.UpdateAim;

public class UpdateAimCommandHandler(
    IValidator<UpdateAimCommand> validator,
    IAimRepository aimRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IPatchMapper patchMapper,
    IMapper mapper)
{
    public async Task<Result<AimDto>> HandleAsync(UpdateAimCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<AimDto>(validationResult.ToValidationError());

        var aim = await aimRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (aim is null)
            return Result.Failure<AimDto>(AimErrors.NotFound(command.Id));

        patchMapper.PatchInto(command, aim);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<AimDto>(aim);
    }
}
