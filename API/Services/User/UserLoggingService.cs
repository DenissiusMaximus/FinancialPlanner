using API.Dtos;

namespace API.Services.User;

public class UserLoggingService(IUserService innerService, ILoggingService loggingService) : IUserService
{
    public async Task<AuthUserDto?> CreateUser(string name, string email, string password)
    {
        var result = await innerService.CreateUser(name, email, password);

        loggingService.WriteLog(result != null
            ? $"User {name}, {email} created successfully."
            : $"Failed to create user {name}, {email}.");

        return result;
    }

    public async Task<AuthUserDto?> LoginUser(string email, string password)
    {
        var result = await innerService.LoginUser(email, password);
        loggingService.WriteLog(result != null
            ? $"User {email} logged in successfully."
            : $"Failed to log in user {email}.");
        return result;
    }
}
