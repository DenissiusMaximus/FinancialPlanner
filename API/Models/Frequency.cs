using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Frequency
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Days { get; set; }

    public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();
}
