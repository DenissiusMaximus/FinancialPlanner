using API.Dtos;

namespace API.Models;

public partial class PlannedTransactionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateOnly StartDate { get; set; }

    public int UserId { get; set; }

    public virtual CategoryDto? Category { get; set; }

    public virtual CurrencyDto Currency { get; set; } = null!;

    public virtual FrequencyDto Frequency { get; set; } = null!;

    public virtual SourceDtoLookup Source { get; set; } = null!;

    public virtual TransactionTypeDto TransactionType { get; set; } = null!;
}

