namespace API.Services.Jwt;

public interface IJwtService
{
	Task<string?> RefreshToken(string refreshToken);
    
    // DEV ONLY
    string GenerateDevAccessToken(int id);
}