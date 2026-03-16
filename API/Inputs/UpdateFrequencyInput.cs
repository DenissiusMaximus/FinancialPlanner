namespace API.Inputs;

public class UpdateFrequencyInput
{
    public string? Name { get; set; } = null!;

    public int? IntervalUnitId { get; set; }

    public int? IntervalValue { get; set; }
}