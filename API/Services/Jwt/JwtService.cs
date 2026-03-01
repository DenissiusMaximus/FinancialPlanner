using API.Utils.JwtProvider;

namespace API.Services.Jwt;

public class JwtService(IJwtProvider jwtProvider, AppDbContext context) : IJwtService
{
    // DEV ONLY
    public async Task<string> GenerateDevAccessToken(int id)
    {
        return jwtProvider.GenerateDevAccessToken(id);
    }

    public Task<string?> RefreshToken(string refreshToken)
    {
        return jwtProvider.RefreshToken(refreshToken);
    }
}