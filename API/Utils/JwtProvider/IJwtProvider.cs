
namespace API.Utils.JwtProvider;

public interface IJwtProvider
{
	string GenerateAccessToken(int id);
	string GenerateRefreshToken(int id);
	Task<string?> RefreshToken(string token);
	Task<bool> AddTokenToBlacklist(string token);
}

