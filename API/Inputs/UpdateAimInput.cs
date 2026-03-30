namespace API.Inputs;

public class UpdateAimInput
{
    public string? Name { get; set; }

    public decimal? Amount { get; set; }

    public int? Priority { get; set; }

    public int? CurrencyId { get; set; }
}

