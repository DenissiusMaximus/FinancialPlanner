namespace FinancialPlanner.Application.Features.Users.Dtos;

public class AuthUserDto
{
    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}
