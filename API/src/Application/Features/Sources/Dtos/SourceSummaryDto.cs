using FinancialPlanner.Application.Common.Dtos;

namespace FinancialPlanner.Application.Features.Sources.Dtos;

public class SourceSummaryDto
{
    public decimal Total { get; set; }

    public IReadOnlyCollection<SourceDtoLookup> Sources { get; set; } = [];
}
