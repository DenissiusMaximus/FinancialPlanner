
namespace API.Utils.JwtProvider;

public interface IJwtProvider
{
	string GenerateAccessToken(int id);
	string GenerateRefreshToken(int id);
    
    // DEV ONLY
    string GenerateDevAccessToken(int id);
	Task<string?> RefreshAccessTokenAsync(string token);
	Task<bool> AddTokenToBlacklistAsync(string token);
}

