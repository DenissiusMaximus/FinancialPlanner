namespace API.Services.Aim;

public class GetAimsInput
{
    public int Limit { get; set; } = 10;
    public int Offset { get; set; } = 0;
    public List<int>? SourceIds { get; set; }
    public bool ClosedOnly { get; set; } = false;
    public bool SortDescending { get; set; } = true;
    public AimSortBy SortBy { get; set; } = AimSortBy.Amount;
}