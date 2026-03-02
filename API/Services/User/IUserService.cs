using API.Dtos;

namespace API.Services;

public interface IUserService
{
    Task<AuthUserDto?> CreateUser(string name, string email, string password);
    Task<AuthUserDto?> LoginUser(string email, string password);
    Task<bool> LogoutUser(string refreshToken);
}
