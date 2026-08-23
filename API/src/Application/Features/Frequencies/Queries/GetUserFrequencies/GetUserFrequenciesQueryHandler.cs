using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Frequencies.Queries.GetUserFrequencies;

public class GetUserFrequenciesQueryHandler(IFrequencyRepository frequencyRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<IReadOnlyCollection<FrequencyDto>>> HandleAsync(GetUserFrequenciesQuery query, CancellationToken ct)
    {
        var frequencies = await frequencyRepository.GetUserOwnedAsync(currentUser.RequiredUserId, ct);

        IReadOnlyCollection<FrequencyDto> dtos = mapper.Map<List<FrequencyDto>>(frequencies);

        return Result.Success(dtos);
    }
}
