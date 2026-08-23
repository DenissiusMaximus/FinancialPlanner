using FinancialPlanner.Application.Abstractions;

namespace FinancialPlanner.Api.Security;

public class CurrentUserContext(IHttpContextAccessor accessor) : ICurrentUserContext
{
    public int RequiredUserId =>
        accessor.HttpContext?.User.GetRequiredUserId()
            ?? throw new UnauthorizedAccessException("User ID not found in context.");

    public int? UserIdOrDefault => accessor.HttpContext?.User.GetUserId();
}
