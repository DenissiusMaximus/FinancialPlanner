using API.Utils.JwtProvider;

namespace API.Services;

public class JwtService(IJwtProvider jwtProvider) : IJwtService
{
  public async Task<string?> RefreshToken(string refreshToken)
  {
    var validateRefreshToken = await jwtProvider.ValidateRefreshToken(refreshToken);

    if (validateRefreshToken != null)
    {
      var newAccessToken = jwtProvider.GenerateAccessToken(validateRefreshToken.Value);
      return newAccessToken;
    }

    return null;
  }
}