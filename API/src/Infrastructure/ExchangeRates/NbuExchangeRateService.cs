using System.Net.Http.Json;
using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinancialPlanner.Infrastructure.ExchangeRates;

public sealed class NbuExchangeRateService(
    HttpClient httpClient,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork,
    ILogger<NbuExchangeRateService> logger) : IExchangeRateService
{
    private const string UsdCurrencyCode = "USD";
    private const string BaseCurrencyCode = "UAH";
    private const string RequestUri = "NBUStatService/v1/statdirectory/exchange?json";

    public async Task RefreshAsync(CancellationToken ct)
    {
        var nbuRates = await httpClient.GetFromJsonAsync<List<NbuExchangeRateDto>>(RequestUri, ct);
        if (nbuRates is null || nbuRates.Count == 0)
        {
            logger.LogWarning("NBU exchange rate response was empty; rates were not updated.");
            return;
        }

        var uahPerUsd = nbuRates.FirstOrDefault(r => r.CurrencyCode == UsdCurrencyCode)?.Rate;
        if (uahPerUsd is null or 0)
        {
            logger.LogWarning("NBU response did not contain a USD rate; rates were not updated.");
            return;
        }

        var uahRatesByCode = nbuRates.ToDictionary(r => r.CurrencyCode, r => r.Rate);

        var currencies = await currencyRepository.GetAllTrackedAsync(ct);

        foreach (var currency in currencies)
        {
            if (currency.Name == UsdCurrencyCode)
            {
                currency.UsdExchangeRate = 1.0000m;
                continue;
            }

            if (currency.Name == BaseCurrencyCode)
            {
                currency.UsdExchangeRate = Math.Round(1 / uahPerUsd.Value, 4);
                continue;
            }

            if (uahRatesByCode.TryGetValue(currency.Name, out var uahPerUnit))
                currency.UsdExchangeRate = Math.Round(uahPerUnit / uahPerUsd.Value, 4);
            else
                logger.LogWarning("NBU response did not contain a rate for currency {Currency}; it was left unchanged.", currency.Name);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
