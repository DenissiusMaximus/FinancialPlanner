using API.Models;

namespace API.Services.Currency;

public interface ICurrencyService
{
    Task<IReadOnlyCollection<CurrencyDto>> GetAllCurrencies();
    Task<CurrencyDto?> GetCurrencyById(int id);
}