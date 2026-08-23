namespace FinancialPlanner.Application.Common.Dtos;

public class CurrencyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal UsdExchangeRate { get; set; }
}
