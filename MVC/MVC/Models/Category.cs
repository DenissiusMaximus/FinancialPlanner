using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
