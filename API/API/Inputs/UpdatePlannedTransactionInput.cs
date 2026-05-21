namespace API.Models;

public partial class UpdatePlannedTransactionInput
{
    public string? Name { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? StartDate { get; set; }

    public int? CurrencyId { get; set; }

    public int? TransactionTypeId { get; set; }

    public int? CategoryId { get; set; }

    public int? SourceId { get; set; }

    public int? FrequencyId { get; set; }
}
