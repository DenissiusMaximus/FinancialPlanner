using API.Services.Transaction;

namespace API.Services;

public class GetUserTransactionsInput
{
    public int Offset { get; set; } = 0;
    public int Limit { get; set; } = 20;
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public TransactionSortBy? SortBy { get; set; }
    public int? CategoryId { get; set; }
    public bool SortDescending { get; set; } = false;
}