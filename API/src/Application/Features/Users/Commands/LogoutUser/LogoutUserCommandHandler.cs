using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Users.Commands.LogoutUser;

public class LogoutUserCommandHandler(
    IJwtProvider jwtProvider,
    IBlacklistedTokenRepository blacklistedTokenRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(LogoutUserCommand command, CancellationToken ct)
    {
        var validated = jwtProvider.ValidateRefreshToken(command.RefreshToken);
        if (validated is null)
            return Result.Failure(UserErrors.LogoutFailed);

        if (await blacklistedTokenRepository.ExistsAsync(validated.Jti, ct))
            return Result.Success();

        await blacklistedTokenRepository.AddAsync(new BlacklistedToken
        {
            Jti = validated.Jti,
            ExpiryDate = validated.ExpiresAtUtc
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
