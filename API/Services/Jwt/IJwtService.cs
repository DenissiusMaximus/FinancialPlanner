namespace API.Services;

public interface IJwtService
{
	Task<string?> RefreshToken(string refreshToken);
}