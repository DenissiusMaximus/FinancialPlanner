namespace API.Services;

public interface IJwtService
{
	Task<string?> RefreshToken(string refreshToken);
    
    // DEV ONLY
    Task<string> GenerateDevAccessToken(int id);
}