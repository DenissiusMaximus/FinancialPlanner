using System.Security.Claims;

namespace API.Extensions;

public static class UserPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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