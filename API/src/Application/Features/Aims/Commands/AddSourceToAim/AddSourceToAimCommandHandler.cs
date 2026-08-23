using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Aims.Commands.AddSourceToAim;

public class AddSourceToAimCommandHandler(
    IAimRepository aimRepository,
    ISourceRepository sourceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<SourceDtoLookup>> HandleAsync(AddSourceToAimCommand command, CancellationToken ct)
    {
        var userId = currentUser.RequiredUserId;

        var aim = await aimRepository.GetByIdAsync(command.AimId, userId, ct);
        if (aim is null)
            return Result.Failure<SourceDtoLookup>(AimErrors.NotFound(command.AimId));

        var source = await sourceRepository.GetByIdAsync(command.SourceId, userId, ct);
        if (source is null)
            return Result.Failure<SourceDtoLookup>(SourceErrors.NotFound(command.SourceId));

        var existingLink = await aimRepository.GetSourceLinkAsync(command.AimId, command.SourceId, ct);
        if (existingLink is not null)
            return Result.Failure<SourceDtoLookup>(AimErrors.SourceAlreadyLinked(command.AimId, command.SourceId));

        aimRepository.AddSourceLink(new SourceAim { AimId = command.AimId, SourceId = command.SourceId });
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<SourceDtoLookup>(source);
    }
}
