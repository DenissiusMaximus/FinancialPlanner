namespace FinancialPlanner.Application.Abstractions;

public interface ICurrentUserContext
{
    int RequiredUserId { get; }

    int? UserIdOrDefault { get; }
}
