using API.Models;
using API.Utils.Notification;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Currency;

public class CurrencyService(AppDbContext context, NotificationContext notificationContext) : ICurrencyService
{
    public async Task<List<CurrencyDto>> GetAllCurrencies()
    {
        var currencies = await context.Currencies.AsNoTracking().ToListAsync();

        return [.. currencies.Select(CreateCurrencyDto)];
    }

    public async Task<CurrencyDto?> GetCurrencyById(int id)
    {
        var currency = await context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        if(currency == null)
        {
            notificationContext.AddNotification("Currency not found.", ErrorType.NotFound);
            return null;
        }

        return CreateCurrencyDto(currency);
    }

    private CurrencyDto CreateCurrencyDto(Models.Currency currency)
    {
        return new CurrencyDto
        {
            Id = currency.Id,
            Name = currency.Name,
            UsdExchangeRate = currency.UsdExchangeRate
        };
    }
}