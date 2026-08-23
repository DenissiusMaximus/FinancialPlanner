using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Sources.Commands.UpdateSource;

public class UpdateSourceCommandHandler(
    IValidator<UpdateSourceCommand> validator,
    ISourceRepository sourceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IPatchMapper patchMapper,
    IMapper mapper)
{
    public async Task<Result<SourceDtoLookup>> HandleAsync(UpdateSourceCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<SourceDtoLookup>(validationResult.ToValidationError());

        var source = await sourceRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (source is null)
            return Result.Failure<SourceDtoLookup>(SourceErrors.NotFound(command.Id));

        patchMapper.PatchInto(command, source);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<SourceDtoLookup>(source);
    }
}
