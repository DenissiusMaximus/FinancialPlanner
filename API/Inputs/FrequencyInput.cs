namespace API.Inputs;

public class FrequencyInput
{
    public string Name { get; set; } = null!;

    public int IntervalUnitId { get; set; }

    public int IntervalValue { get; set; }
}