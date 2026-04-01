namespace API.Inputs;

public class UpdateFrequencyInput
{
    public string? Name { get; set; }

    public int? IntervalUnitId { get; set; }

    public int? IntervalValue { get; set; }
}