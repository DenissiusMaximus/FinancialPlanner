namespace API.Inputs;

public class CreateTransactionInput
{
    public decimal Amount { get; set; }

    public string? Comment { get; set; }

    public DateOnly Date { get; set; }

    public int? CategoryId { get; set; }

    public int SourceId { get; set; }

    public int? DestinationSourceId { get; set; }

    public int CurrencyId { get; set; }

    public int TransactionTypeId { get; set; }
}

