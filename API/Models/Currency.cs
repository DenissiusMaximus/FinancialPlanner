using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Currency
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal UsdExchangeRate { get; set; }

    public virtual ICollection<Aim> Aims { get; set; } = new List<Aim>();

    public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();

    public virtual ICollection<Source> Sources { get; set; } = new List<Source>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
