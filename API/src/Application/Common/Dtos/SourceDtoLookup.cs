namespace FinancialPlanner.Application.Common.Dtos;

public class SourceDtoLookup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int UserId { get; set; }

    public CurrencyDto Currency { get; set; } = null!;

    public bool IsArchived { get; set; }
}
