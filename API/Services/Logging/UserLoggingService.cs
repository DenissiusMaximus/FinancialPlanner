using API.Dtos;

namespace API.Services.User;

public class UserLoggingService(IUserService innerService, ILogger<UserLoggingService> logger) : IUserService
{
    public async Task<AuthUserDto?> CreateUser(string name, string email, string password)
    {
        var result = await innerService.CreateUser(name, email, password);
        
        if(result != null)
            logger.LogInformation("User created with email: {Email}", email);

        return result;
    }

    public async Task<AuthUserDto?> LoginUser(string email, string password)
    {
        var result = await innerService.LoginUser(email, password);
    
        if(result != null)
            logger.LogInformation("User logged in with email: {Email}", email);

        return result;
    }

    public async Task<bool> LogoutUser(string refreshToken)
    {
        var result = await innerService.LogoutUser(refreshToken);

        if(result)
            logger.LogInformation("User logged out");
        
        return result;
    }
}
