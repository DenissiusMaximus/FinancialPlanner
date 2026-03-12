using API.Models;

namespace API.Services.Currency;

public interface ICurrencyService
{
    public Task<List<CurrencyDto>> GetAllCurrencies();
    public Task<CurrencyDto?> GetCurrencyById(int id);
}