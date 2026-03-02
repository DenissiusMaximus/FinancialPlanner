using System;

namespace API.Dtos;

public class CreateSourceInput
{
    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int CurrencyId { get; set; }
}
