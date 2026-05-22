using System.Security.Claims;
using API;
using API.Extensions;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Utils.UserContext;

public class CurrentUserContext(IHttpContextAccessor accessor, AppDbContext context) : ICurrentUserContext
{
    public int RequiredUserId
    {
        get
        {
            var resolvedUserId = ResolveUserId();

            return resolvedUserId
                ?? throw new UnauthorizedAccessException("User ID not found in context");
        }
    }

    public int? UserIdOrDefault => ResolveUserId();

    private int? ResolveUserId()
    {
        var principal = accessor.HttpContext?.User;
        var userIdFromClaim = principal?.GetUserId();

        if (userIdFromClaim.HasValue)
        {
            return userIdFromClaim.Value;
        }

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst("email")?.Value
                    ?? principal.FindFirst(ClaimTypes.Name)?.Value
                    ?? principal.Identity?.Name;

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var existingUserId = context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => (int?)u.Id)
            .FirstOrDefault();

        if (existingUserId.HasValue)
        {
            return existingUserId.Value;
        }

        var newUser = new User
        {
            Name = email,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"))
        };

        context.Users.Add(newUser);

        try
        {
            context.SaveChanges();
            return newUser.Id;
        }
        catch (DbUpdateException)
        {
            return context.Users
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => (int?)u.Id)
                .FirstOrDefault();
        }
    }
}