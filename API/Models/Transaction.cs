using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string? Comment { get; set; }

    public DateOnly Date { get; set; }

    public int UserId { get; set; }

    public int? CategoryId { get; set; }

    public int SourceId { get; set; }

    public int? DestinationSourceId { get; set; }

    public int CurrencyId { get; set; }

    public int TransactionTypeId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual Source? DestinationSource { get; set; }

    public virtual Source Source { get; set; } = null!;

    public virtual TransactionType TransactionType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
