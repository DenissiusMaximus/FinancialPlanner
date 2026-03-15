namespace API.Utils.UserContext;

public interface ICurrentUserContext
{
    int RequiredUserId { get; }
    int? UserIdOrDefault { get; }
}