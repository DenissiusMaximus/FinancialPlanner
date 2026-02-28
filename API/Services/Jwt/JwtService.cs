using API.Models;
using API.Utils.JwtProvider;

namespace API.Services.Jwt;

public class JwtService(IJwtProvider jwtProvider, AppDbContext context) : IJwtService
{
    public Task<string?> RefreshToken(string refreshToken)
    {
        return jwtProvider.RefreshToken(refreshToken);
    }
}