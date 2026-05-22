using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace API.Extensions;

public static class UserPrincipalExtensions
{
    private const string AppUserIdClaimType = "app_user_id";

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdString = user.FindFirst(AppUserIdClaimType)?.Value
                           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? user.FindFirst("sub")?.Value;

        if (int.TryParse(userIdString, out int userId))
        {
            return userId;
        }

        return null;
    }

    public static int GetRequiredUserId(this ClaimsPrincipal user)
    {
        var userId = GetUserId(user) ?? throw new Exception("User ID not found in claims.");
        return userId;
    }
}