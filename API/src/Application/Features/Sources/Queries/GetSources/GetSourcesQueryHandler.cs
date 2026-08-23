using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Sources.Queries.GetSources;

public class GetSourcesQueryHandler(ISourceRepository sourceRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<IReadOnlyCollection<SourceDtoLookup>>> HandleAsync(GetSourcesQuery query, CancellationToken ct)
    {
        var sources = await sourceRepository.GetAllAsync(currentUser.RequiredUserId, ct);

        IReadOnlyCollection<SourceDtoLookup> dtos = mapper.Map<List<SourceDtoLookup>>(sources);

        return Result.Success(dtos);
    }
}
