using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.Sources.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Sources.Queries.GetSourceSummary;

public class GetSourceSummaryQueryHandler(ISourceRepository sourceRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<SourceSummaryDto>> HandleAsync(GetSourceSummaryQuery query, CancellationToken ct)
    {
        var sources = await sourceRepository.GetAllAsync(currentUser.RequiredUserId, ct);

        var dtos = mapper.Map<List<SourceDtoLookup>>(sources);

        return new SourceSummaryDto
        {
            Total = dtos.Sum(s => s.Amount),
            Sources = dtos
        };
    }
}
