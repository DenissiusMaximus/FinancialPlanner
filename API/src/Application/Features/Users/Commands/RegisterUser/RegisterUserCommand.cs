namespace FinancialPlanner.Application.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Name, string Email, string Password);
