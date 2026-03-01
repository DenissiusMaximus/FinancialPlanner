namespace API.Services;

public interface IJwtService
{
	Task<string?> RefreshToken(string refreshToken);
    Task<string> GenerateDevAccessToken(int id);
}