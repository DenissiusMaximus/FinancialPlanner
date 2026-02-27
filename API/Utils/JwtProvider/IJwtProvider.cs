
namespace API.Utils.JwtProvider;

public interface IJwtProvider
{
	string GenerateAccessToken(int id);
	string GenerateRefreshToken(int id);
	Task<int?> ValidateRefreshToken(string token);
}

