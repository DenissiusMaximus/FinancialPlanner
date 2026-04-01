using System;

namespace API.Inputs;

public class CreateAimInput
{
    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int Priority { get; set; }

    public int? CurrencyId { get; set; }
}

