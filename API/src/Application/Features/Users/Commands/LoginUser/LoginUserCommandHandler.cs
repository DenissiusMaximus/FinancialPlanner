using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Users.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;

namespace FinancialPlanner.Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler(
    IValidator<LoginUserCommand> validator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider)
{
    public async Task<Result<AuthUserDto>> HandleAsync(LoginUserCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<AuthUserDto>(validationResult.ToValidationError());

        var user = await userRepository.GetByEmailAsync(command.Email, ct);
        if (user is null || !passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            return Result.Failure<AuthUserDto>(UserErrors.InvalidCredentials);

        return new AuthUserDto
        {
            AccessToken = jwtProvider.GenerateAccessToken(user.Id),
            RefreshToken = jwtProvider.GenerateRefreshToken(user.Id)
        };
    }
}
