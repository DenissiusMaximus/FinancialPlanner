using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Repositories;

namespace FinancialPlanner.Application.Features.Jwt.Commands.RefreshAccessToken;

public class RefreshAccessTokenCommandHandler(
    IJwtProvider jwtProvider,
    IBlacklistedTokenRepository blacklistedTokenRepository)
{
    public async Task<Result<string>> HandleAsync(RefreshAccessTokenCommand command, CancellationToken ct)
    {
        var validated = jwtProvider.ValidateRefreshToken(command.RefreshToken);
        if (validated is null)
            return Result.Failure<string>(UserErrors.RefreshTokenInvalid);

        if (await blacklistedTokenRepository.ExistsAsync(validated.Jti, ct))
            return Result.Failure<string>(UserErrors.RefreshTokenInvalid);

        return jwtProvider.GenerateAccessToken(validated.UserId);
    }
}
