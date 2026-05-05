namespace API.Services;

public class GetUserPlannedTransactionsInput
{
    public int Offset { get; set; } = 0;
    public int Limit { get; set; } = 20;
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool SortDescending { get; set; } = true;
}