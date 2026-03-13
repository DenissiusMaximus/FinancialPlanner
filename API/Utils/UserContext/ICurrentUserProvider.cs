namespace API.Utils.UserContext;

public interface ICurrentUserProvider
{
    int RequiredUserId { get; }
    int? UserIdOrDefault { get; }
}