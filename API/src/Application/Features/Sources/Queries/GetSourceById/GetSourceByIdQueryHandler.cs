using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Features.Sources.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Sources.Queries.GetSourceById;

public class GetSourceByIdQueryHandler(ISourceRepository sourceRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<SourceDtoDetailed>> HandleAsync(GetSourceByIdQuery query, CancellationToken ct)
    {
        var source = await sourceRepository.GetByIdAsync(query.Id, currentUser.RequiredUserId, ct);
        if (source is null)
            return Result.Failure<SourceDtoDetailed>(SourceErrors.NotFound(query.Id));

        return mapper.Map<SourceDtoDetailed>(source);
    }
}
