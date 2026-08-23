using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinancialPlanner.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinancialPlanner.Infrastructure.Security;

public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
    public string GenerateDevAccessToken(int userId)
    {
#if DEBUG
        return GenerateToken(userId, () => DateTime.UtcNow.AddYears(100), options.Value.SecretAccess);
#else
        throw new NotSupportedException("Dev access tokens are only available in DEBUG builds.");
#endif
    }

    public string GenerateAccessToken(int userId)
        => GenerateToken(userId, () => DateTime.UtcNow.Add(options.Value.AccessTokenExpiration), options.Value.SecretAccess);

    public string GenerateRefreshToken(int userId)
        => GenerateToken(userId, () => DateTime.UtcNow.Add(options.Value.RefreshTokenExpiration), options.Value.SecretRefresh);

    public JwtValidationResult? ValidateRefreshToken(string token)
        => ValidateToken(token, options.Value.SecretRefresh);

    private static string GenerateToken(int userId, Func<DateTime> addLifetime, string secret)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: addLifetime(),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static JwtValidationResult? ValidateToken(string token, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            var jtiClaim = principal.FindFirst(JwtRegisteredClaimNames.Jti);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId)
                && jtiClaim != null && !string.IsNullOrEmpty(jtiClaim.Value))
            {
                return new JwtValidationResult(userId, jtiClaim.Value, validatedToken.ValidTo);
            }
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }

        return null;
    }
}
