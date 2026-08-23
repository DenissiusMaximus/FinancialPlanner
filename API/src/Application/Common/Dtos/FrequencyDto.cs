namespace FinancialPlanner.Application.Common.Dtos;

public class FrequencyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? UserId { get; set; }

    public int IntervalValue { get; set; }

    public IntervalUnitDto? IntervalUnit { get; set; }
}
