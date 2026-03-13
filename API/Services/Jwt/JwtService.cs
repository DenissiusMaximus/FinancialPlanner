using API.Utils.JwtProvider;

namespace API.Services.Jwt;

public class JwtService(IJwtProvider jwtProvider, AppDbContext context) : IJwtService
{
    // DEV ONLY
    public string GenerateDevAccessToken(int id)
    {
        return jwtProvider.GenerateDevAccessToken(id);
    }

    public async Task<string?> RefreshToken(string refreshToken)
    {
        return await jwtProvider.RefreshTokenAsync(refreshToken);
    }
}