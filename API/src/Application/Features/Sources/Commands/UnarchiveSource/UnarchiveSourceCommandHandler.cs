using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Sources.Commands.UnarchiveSource;

public class UnarchiveSourceCommandHandler(
    ISourceRepository sourceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IMapper mapper)
{
    public async Task<Result<SourceDtoLookup>> HandleAsync(UnarchiveSourceCommand command, CancellationToken ct)
    {
        var source = await sourceRepository.GetByIdAsync(command.Id, currentUser.RequiredUserId, ct);
        if (source is null)
            return Result.Failure<SourceDtoLookup>(SourceErrors.NotFound(command.Id));

        source.IsArchived = false;
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<SourceDtoLookup>(source);
    }
}
