using FinancialPlanner.Application.Common.Dtos;

namespace FinancialPlanner.Application.Features.Aims.Dtos;

public class AimDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int Priority { get; set; }

    public int UserId { get; set; }

    public bool IsClosed { get; set; }

    public CurrencyDto? Currency { get; set; }

    public ICollection<SourceDtoLookup>? Sources { get; set; }

    public AimProgressDto? Progress { get; set; }
}
