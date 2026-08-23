using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Currencies.Queries.GetCurrencyById;

public class GetCurrencyByIdQueryHandler(ICurrencyRepository currencyRepository, IMapper mapper)
{
    public async Task<Result<CurrencyDto>> HandleAsync(GetCurrencyByIdQuery query, CancellationToken ct)
    {
        var currency = await currencyRepository.GetByIdAsync(query.Id, ct);

        if (currency is null)
            return Result.Failure<CurrencyDto>(CurrencyErrors.NotFound(query.Id));

        return mapper.Map<CurrencyDto>(currency);
    }
}
