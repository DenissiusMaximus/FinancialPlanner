using System;
using System.Collections.Generic;

namespace API.Models;

public partial class PlannedTransaction
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateOnly StartDate { get; set; }

    public int CurrencyId { get; set; }

    public int UserId { get; set; }

    public int TransactionTypeId { get; set; }

    public int? CategoryId { get; set; }

    public int SourceId { get; set; }

    public int FrequencyId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual Frequency Frequency { get; set; } = null!;

    public virtual Source Source { get; set; } = null!;

    public virtual TransactionType TransactionType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
