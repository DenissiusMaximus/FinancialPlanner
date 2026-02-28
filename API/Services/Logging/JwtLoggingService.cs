namespace API.Services;

public class JwtLoggingService(
    IJwtService innerService,
    ILogger<JwtLoggingService> logger,
    IHttpContextAccessor httpContextAccessor)
    : IJwtService
{
    public async Task<string?> RefreshToken(string refreshToken)
    {
        var result = await innerService.RefreshToken(refreshToken);

        var ipAdress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

        if (result == null)
            logger.LogWarning("Failed to refresh access token with provided refresh token.IP address: {IpAddress}", ipAdress);

        return result;
    }
}