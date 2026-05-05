using API.Dtos;
using API.Models;

namespace API.Inputs;

public class AimDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int Priority { get; set; }

    public int UserId { get; set; }

    public bool IsClosed { get; set; }

    public virtual CurrencyDto? Currency { get; set; }

    public virtual ICollection<SourceDtoLookup>? Sources { get; set; }

    public virtual AimProgressDto? Progress { get; set; }

}
