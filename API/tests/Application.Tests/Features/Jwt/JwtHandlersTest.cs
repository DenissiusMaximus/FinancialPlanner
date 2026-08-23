using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Features.Jwt.Commands.RefreshAccessToken;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;
using Moq;

namespace FinancialPlanner.Application.Tests.Features.Jwt;

public class JwtHandlersTest : BaseTest
{
    [Fact]
    public async Task RefreshAccessToken_ReturnsNewAccessToken_WhenTokenIsValid()
    {
        var dbContext = GetInMemoryDbContext();

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.ValidateRefreshToken("refresh"))
            .Returns(new JwtValidationResult(7, "jti-7", DateTime.UtcNow.AddDays(1)));
        jwtProviderMock.Setup(j => j.GenerateAccessToken(7)).Returns("access");

        var handler = new RefreshAccessTokenCommandHandler(jwtProviderMock.Object, new BlacklistedTokenRepository(dbContext));

        var result = await handler.HandleAsync(new RefreshAccessTokenCommand("refresh"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("access");
    }

    [Fact]
    public async Task RefreshAccessToken_Fails_WhenTokenIsInvalid()
    {
        var dbContext = GetInMemoryDbContext();

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.ValidateRefreshToken("bad")).Returns((JwtValidationResult?)null);

        var handler = new RefreshAccessTokenCommandHandler(jwtProviderMock.Object, new BlacklistedTokenRepository(dbContext));

        var result = await handler.HandleAsync(new RefreshAccessTokenCommand("bad"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.RefreshTokenInvalid.Code);
    }

    [Fact]
    public async Task RefreshAccessToken_Fails_WhenTokenIsBlacklisted()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.BlacklistedTokens.Add(new FinancialPlanner.Domain.Entities.BlacklistedToken { Jti = "jti-9", ExpiryDate = DateTime.UtcNow.AddDays(1) });
        await dbContext.SaveChangesAsync();

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.ValidateRefreshToken("refresh"))
            .Returns(new JwtValidationResult(9, "jti-9", DateTime.UtcNow.AddDays(1)));

        var handler = new RefreshAccessTokenCommandHandler(jwtProviderMock.Object, new BlacklistedTokenRepository(dbContext));

        var result = await handler.HandleAsync(new RefreshAccessTokenCommand("refresh"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.RefreshTokenInvalid.Code);
    }
}
