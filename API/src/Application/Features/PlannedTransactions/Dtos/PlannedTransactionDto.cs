using FinancialPlanner.Application.Common.Dtos;

namespace FinancialPlanner.Application.Features.PlannedTransactions.Dtos;

public class PlannedTransactionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime StartDate { get; set; }

    public int UserId { get; set; }

    public CategoryDto? Category { get; set; }

    public CurrencyDto Currency { get; set; } = null!;

    public FrequencyDto Frequency { get; set; } = null!;

    public SourceDtoLookup Source { get; set; } = null!;

    public TransactionTypeDto TransactionType { get; set; } = null!;
}
