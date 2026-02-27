namespace API.Utils;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        return hashedPassword 
            ?? throw new Exception("Failed to hash password");
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}