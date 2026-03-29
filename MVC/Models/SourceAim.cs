using System;
using System.Collections.Generic;

namespace API.Models;

public partial class SourceAim
{
    public int Id { get; set; }

    public int SourceId { get; set; }

    public int AimId { get; set; }

    public virtual Aim Aim { get; set; } = null!;

    public virtual Source Source { get; set; } = null!;
}
