namespace FinancialPlanner.Application.Abstractions;

public interface IJwtProvider
{
    string GenerateAccessToken(int userId);

    string GenerateRefreshToken(int userId);

    string GenerateDevAccessToken(int userId);

    JwtValidationResult? ValidateRefreshToken(string token);
}

public sealed record JwtValidationResult(int UserId, string Jti, DateTime ExpiresAtUtc);
