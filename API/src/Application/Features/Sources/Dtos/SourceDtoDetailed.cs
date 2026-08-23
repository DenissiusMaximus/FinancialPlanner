using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Application.Features.Aims.Dtos;

namespace FinancialPlanner.Application.Features.Sources.Dtos;

public class SourceDtoDetailed
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }

    public int UserId { get; set; }

    public CurrencyDto Currency { get; set; } = null!;

    public bool IsArchived { get; set; }

    public IReadOnlyCollection<AimDto>? Aims { get; set; }
}
