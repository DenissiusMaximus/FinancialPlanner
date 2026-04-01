using API.Dtos;

namespace API.Models;

public class TransactionDto
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string? Comment { get; set; }

    public DateOnly Date { get; set; }

    public int UserId { get; set; }
    
    public virtual CategoryDto? Category { get; set; }

    public virtual CurrencyDto Currency { get; set; } = null!;

    public virtual SourceDtoLookup? DestinationSource { get; set; }

    public virtual SourceDtoLookup Source { get; set; } = null!;

    public virtual TransactionTypeDto TransactionType { get; set; } = null!;
}