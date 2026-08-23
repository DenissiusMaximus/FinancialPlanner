using System.Security.Claims;

namespace FinancialPlanner.Api.Security;

public static class UserPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(value, out var id) ? id : null;
    }

    public static int GetRequiredUserId(this ClaimsPrincipal user)
        => user.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found in claims.");
}
