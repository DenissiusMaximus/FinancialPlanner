using API.Extensions;

namespace API.Utils.UserContext;

public class CurrentUserProvider(IHttpContextAccessor accessor) : ICurrentUserProvider
{
    public int RequiredUserId => accessor.HttpContext?.User.GetRequiredUserId() ?? throw new UnauthorizedAccessException("User ID not found in context");

    public int? UserIdOrDefault => accessor.HttpContext?.User.GetUserId();
}