namespace FinancialPlanner.Domain.Entities;

public class IntervalUnit
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Frequency> Frequencies { get; set; } = new List<Frequency>();
}
