using FinancialPlanner.Domain.Common;

namespace FinancialPlanner.Domain.Errors;

public static class UserErrors
{
    public static Error NotFound(int id) => new(
        "Users.NotFound",
        $"User with id '{id}' was not found.",
        ErrorType.NotFound);

    public static Error EmailAlreadyUsed(string email) => new(
        "Users.EmailAlreadyUsed",
        $"Email '{email}' is already in use.",
        ErrorType.Conflict);

    public static readonly Error InvalidCredentials = new(
        "Users.InvalidCredentials",
        "The provided credentials are incorrect.",
        ErrorType.Unauthorized);

    public static readonly Error RefreshTokenInvalid = new(
        "Users.RefreshTokenInvalid",
        "The provided refresh token is invalid or expired.",
        ErrorType.Unauthorized);

    public static readonly Error LogoutFailed = new(
        "Users.LogoutFailed",
        "Failed to log out with the provided refresh token.",
        ErrorType.Validation);
}
