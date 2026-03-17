using System;
using API.Models;

namespace API.Dtos;

public class SourceDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int UserId { get; set; }

    public CurrencyDto Currency { get; set; }

    public string CurrencyName { get; set; }

    public bool IsArchived { get; set; }
}
