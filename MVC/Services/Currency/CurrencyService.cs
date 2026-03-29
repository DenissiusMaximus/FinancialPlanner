using API.Models;
using API.Utils.Notification;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Currency;

public class CurrencyService(AppDbContext context, NotificationContext notificationContext) : ICurrencyService
{
    public async Task<IReadOnlyCollection<CurrencyDto>> GetAllCurrencies()
    {
        var currencies = await context.Currencies.AsNoTracking().ToListAsync();

        return [.. currencies.Select(c => c.Adapt<CurrencyDto>())];
    }

    public async Task<CurrencyDto?> GetCurrencyById(int id)
    {
        var currency = await context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        if(currency == null)
        {
            notificationContext.AddNotification("Currency not found.", ErrorType.NotFound);
            return null;
        }

        return currency.Adapt<CurrencyDto>();
    }
}