using System;

namespace API.Dtos;

public class UpdateSourceInput
{
    public int Id { get; set; }

    public string? Name { get; set; } = null!;

    public int? CurrencyId { get; set; }
}
