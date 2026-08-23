namespace FinancialPlanner.Infrastructure.Security;

public class JwtOptions
{
    public string SecretAccess { get; set; } = null!;

    public string SecretRefresh { get; set; } = null!;

    public TimeSpan AccessTokenExpiration { get; set; }

    public TimeSpan RefreshTokenExpiration { get; set; }
}
