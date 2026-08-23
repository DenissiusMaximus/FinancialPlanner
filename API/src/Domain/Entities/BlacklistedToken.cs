namespace FinancialPlanner.Domain.Entities;

public class BlacklistedToken
{
    public int Id { get; set; }

    public string Jti { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }
}
