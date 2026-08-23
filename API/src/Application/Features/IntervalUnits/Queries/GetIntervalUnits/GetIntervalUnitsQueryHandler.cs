using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.IntervalUnits.Queries.GetIntervalUnits;

public class GetIntervalUnitsQueryHandler(IIntervalUnitRepository intervalUnitRepository, IMapper mapper)
{
    public async Task<Result<IReadOnlyCollection<IntervalUnitDto>>> HandleAsync(GetIntervalUnitsQuery query, CancellationToken ct)
    {
        var intervalUnits = await intervalUnitRepository.GetAllAsync(ct);

        IReadOnlyCollection<IntervalUnitDto> dtos = mapper.Map<List<IntervalUnitDto>>(intervalUnits);

        return Result.Success(dtos);
    }
}
