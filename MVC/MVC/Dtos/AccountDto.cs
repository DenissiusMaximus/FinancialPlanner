namespace API.Dtos;

public class SourceSummaryDto
{
    public decimal Total { get; set; }
    public IReadOnlyCollection<SourceDtoLookup> Sources { get; set; }
}