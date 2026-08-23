using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Frequencies.Queries.GetFrequencyById;

public class GetFrequencyByIdQueryHandler(IFrequencyRepository frequencyRepository, ICurrentUserContext currentUser, IMapper mapper)
{
    public async Task<Result<FrequencyDto>> HandleAsync(GetFrequencyByIdQuery query, CancellationToken ct)
    {
        var frequency = await frequencyRepository.GetByIdAsync(query.Id, currentUser.RequiredUserId, ct);
        if (frequency is null)
            return Result.Failure<FrequencyDto>(FrequencyErrors.NotFound(query.Id));

        return mapper.Map<FrequencyDto>(frequency);
    }
}
