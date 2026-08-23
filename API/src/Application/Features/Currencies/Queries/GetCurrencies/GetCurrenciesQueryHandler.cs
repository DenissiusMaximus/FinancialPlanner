using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Repositories;
using MapsterMapper;

namespace FinancialPlanner.Application.Features.Currencies.Queries.GetCurrencies;

public class GetCurrenciesQueryHandler(ICurrencyRepository currencyRepository, IMapper mapper)
{
    public async Task<Result<IReadOnlyCollection<CurrencyDto>>> HandleAsync(GetCurrenciesQuery query, CancellationToken ct)
    {
        var currencies = await currencyRepository.GetAllAsync(ct);

        IReadOnlyCollection<CurrencyDto> dtos = mapper.Map<List<CurrencyDto>>(currencies);

        return Result.Success(dtos);
    }
}
