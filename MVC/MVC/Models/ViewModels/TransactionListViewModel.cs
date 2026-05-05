namespace API.Models.ViewModels;

public class TransactionListViewModel
{
    public IEnumerable<TransactionDto> Transactions { get; set; } = Enumerable.Empty<TransactionDto>();
    public string? CurrentCategory { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}
