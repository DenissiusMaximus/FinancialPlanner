using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.IntervalUnits.Queries.GetIntervalUnitById;

public class GetIntervalUnitByIdQueryHandler(IIntervalUnitRepository intervalUnitRepository, IMapper mapper)
{
    public async Task<Result<IntervalUnitDto>> HandleAsync(GetIntervalUnitByIdQuery query, CancellationToken ct)
    {
        var intervalUnit = await intervalUnitRepository.GetByIdAsync(query.Id, ct);

        if (intervalUnit is null)
            return Result.Failure<IntervalUnitDto>(IntervalUnitErrors.NotFound(query.Id));

        return mapper.Map<IntervalUnitDto>(intervalUnit);
    }
}
