using API.Dtos;

namespace API.Services.User;

public class UserLoggingService(IUserService innerService, ILogger<UserLoggingService> logger) : IUserService
{
    public async Task<AuthUserDto?> CreateUser(string name, string email, string password)
    {
        var result = await innerService.CreateUser(name, email, password);
        
        if(result != null)
            logger.LogInformation("User created with email: {Email}", email);
        else
            logger.LogWarning("Failed to create user with email: {Email}", email);

        return result;
    }

    public async Task<AuthUserDto?> LoginUser(string email, string password)
    {
        var result = await innerService.LoginUser(email, password);
    
        if(result != null)
            logger.LogInformation("User logged in with email: {Email}", email);
        else
            logger.LogWarning("Failed to log in user with email: {Email}", email);

        return result;
    }
}
