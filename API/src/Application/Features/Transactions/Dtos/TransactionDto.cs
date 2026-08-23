using FinancialPlanner.Application.Common.Dtos;

namespace FinancialPlanner.Application.Features.Transactions.Dtos;

public class TransactionDto
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string? Comment { get; set; }

    public DateTime Date { get; set; }

    public int UserId { get; set; }

    public CategoryDto? Category { get; set; }

    public CurrencyDto Currency { get; set; } = null!;

    public SourceDtoLookup? DestinationSource { get; set; }

    public SourceDtoLookup Source { get; set; } = null!;

    public TransactionTypeDto TransactionType { get; set; } = null!;
}
