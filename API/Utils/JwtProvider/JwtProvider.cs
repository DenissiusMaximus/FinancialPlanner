using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Utils.JwtProvider;

public class JwtProvider(IOptions<JwtOptions> options, AppDbContext context) : IJwtProvider
{
    public string GenerateAccessToken(int id)
        => GenerateToken(id, () => DateTime.UtcNow.AddMinutes(15), options.Value.SecretAccess);

    public string GenerateRefreshToken(int id)
        => GenerateToken(id, () => DateTime.UtcNow.AddDays(30), options.Value.SecretRefresh);

    public async Task<string?> RefreshToken(string token)
    {
        var result = await ValidateToken(token, options.Value.SecretRefresh);

        if (result == null)
            return null;
        
        var checkTokenBlacklisted = await context.BlacklistedTokens.AnyAsync(t => t.Jti == result.Value.jti);

        if (checkTokenBlacklisted)
            return null;

        return GenerateToken(result.Value.userId, () => DateTime.UtcNow.AddDays(30), options.Value.SecretRefresh);
    }

    public async Task<bool> AddTokenToBlacklist(string token)
    {
        var validatedToken = await ValidateToken(token, options.Value.SecretRefresh);

        if (validatedToken == null)
            return false;

        await context.BlacklistedTokens.AddAsync(new BlacklistedToken
        {
            Jti = validatedToken.Value.jti,
            ExpiryDate = validatedToken.Value.expirationDate
        });

        var result = await context.SaveChangesAsync();

        return result > 0;
    }

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

    private static async Task<(int userId, string jti, DateTime expirationDate)?> ValidateToken(string token, string secret)
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
                return (userId, jtiClaim.Value, validatedToken.ValidTo);
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