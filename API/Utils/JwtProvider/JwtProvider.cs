using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace API.Utils.JwtProvider;

public class JwtProvider : IJwtProvider
{
    private readonly string _accessSecret;
    private readonly string _refreshSecret;

    public JwtProvider(IConfiguration configuration)
    {
        var accessSecret = configuration.GetValue<string>("Jwt:SecretAccess")
                           ?? throw new Exception("JWT access secret not found");
        var refreshSecret = configuration.GetValue<string>("Jwt:SecretRefresh")
                            ?? throw new Exception("JWT refresh secret not found");

        _refreshSecret = refreshSecret;
        _accessSecret = accessSecret;
    }

    public string GenerateAccessToken(int id)
        => GenerateToken(id, () => DateTime.UtcNow.AddMinutes(15), _accessSecret);

    public string GenerateRefreshToken(int id)
        => GenerateToken(id, () => DateTime.UtcNow.AddDays(30), _refreshSecret);

    public async Task<int?> ValidateRefreshToken(string token)
        => await ValidateToken(token, _refreshSecret);

    private static string GenerateToken(int id, Func<DateTime> addLifeTime, string secret)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: addLifeTime(),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<int?> ValidateToken(string token, string secret)
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
            var principal = tokenHandler.ValidateToken(token, parameters, out _);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)
                              ?? principal.FindFirst(ClaimTypes.NameIdentifier);


            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }
        catch (ArgumentException ex )
        {
            return null;
        }
        catch (SecurityTokenException ex)
        {
            return null;
        }

        return null;
    }
}