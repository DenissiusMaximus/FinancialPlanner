using API.Dtos;
using API.Models;
using API.Utils;
using API.Utils.JwtProvider;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UserService(AppDbContext context, IPasswordHasher passwordHasher, IJwtProvider jwtProvider) : IUserService
{
    public async Task<AuthUserDto?> CreateUser(string name, string email, string password)
    {
        var passwordHash = passwordHasher.HashPassword(password);

        var user = new Models.User
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash
        };

        if (await context.Users.AnyAsync(u => u.Email == email))
            return null;

        var createdUser = await context.Users.AddAsync(user);
        var res = await context.SaveChangesAsync();
        if (res == 0)
            return null;

        var accessToken = jwtProvider.GenerateAccessToken(createdUser.Entity.Id);
        var refreshToken = jwtProvider.GenerateRefreshToken(createdUser.Entity.Id);

        return new AuthUserDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthUserDto?> LoginUser(string email, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => email == u.Email);

        if(user == null)
            return null;
        
        var isPasswordValid = passwordHasher.VerifyPassword(password, user.PasswordHash);

        if (!isPasswordValid)
            return null;
        
        var accessToken = jwtProvider.GenerateAccessToken(user.Id);
        var refreshToken = jwtProvider.GenerateRefreshToken(user.Id);

        return new AuthUserDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<bool> Logout(string refreshToken)
    {
        return await jwtProvider.AddTokenToBlacklist(refreshToken);
    }
}