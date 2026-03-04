using System;

namespace API.Dtos;

public class SourceDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int UserId { get; set; }

    public int CurrencyId { get; set; }

    public string CurrencyName { get; set; }

    public bool IsArchived { get; set; }
}
