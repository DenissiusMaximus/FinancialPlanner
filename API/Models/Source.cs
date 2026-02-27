using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Source
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int UserId { get; set; }

    public int CurrencyId { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();

    public virtual ICollection<SourceAim> SourceAims { get; set; } = new List<SourceAim>();

    public virtual ICollection<Transaction> TransactionDestinationSources { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionSources { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
