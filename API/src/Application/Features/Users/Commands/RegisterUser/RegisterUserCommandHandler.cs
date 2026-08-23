using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Validation;
using FinancialPlanner.Application.Features.Users.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;
using FluentValidation;

namespace FinancialPlanner.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler(
    IValidator<RegisterUserCommand> validator,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider)
{
    public async Task<Result<AuthUserDto>> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result.Failure<AuthUserDto>(validationResult.ToValidationError());

        if (await userRepository.ExistsByEmailAsync(command.Email, ct))
            return Result.Failure<AuthUserDto>(UserErrors.EmailAlreadyUsed(command.Email));

        var user = new User
        {
            Name = command.Name,
            Email = command.Email,
            PasswordHash = passwordHasher.HashPassword(command.Password)
        };

        var added = await userRepository.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthUserDto
        {
            AccessToken = jwtProvider.GenerateAccessToken(added.Id),
            RefreshToken = jwtProvider.GenerateRefreshToken(added.Id)
        };
    }
}
