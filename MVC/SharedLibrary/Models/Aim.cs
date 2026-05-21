using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Aim
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int Priority { get; set; }

    public int UserId { get; set; }

    public bool IsClosed { get; set; }

    public int? CurrencyId { get; set; }

    public virtual Currency? Currency { get; set; }

    public virtual ICollection<SourceAim> SourceAims { get; set; } = new List<SourceAim>();

    public virtual User User { get; set; } = null!;
}
