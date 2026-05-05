using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Frequency
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int IntervalValue { get; set; }

    public int? UserId { get; set; }

    public int IntervalUnitId { get; set; }

    public virtual IntervalUnit IntervalUnitNavigation { get; set; } = null!;

    public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();

    public virtual User? User { get; set; }
}
