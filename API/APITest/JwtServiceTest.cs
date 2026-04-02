using API.Services.Jwt;
using API.Utils.JwtProvider;
using FluentAssertions;
using Moq;

namespace APITest;

public class JwtServiceTest : BaseTest
{
    [Fact]
    public async Task RefreshToken_ReturnsNewAccessToken()
    {
        var dbContext = GetInMemoryDbContext();
        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.RefreshAccessTokenAsync("refresh")).ReturnsAsync("access");

        var service = new JwtService(jwtProviderMock.Object, dbContext);

        var result = await service.RefreshToken("refresh");

        result.Should().Be("access");
    }

    [Fact]
    public void GenerateDevAccessToken_ReturnsGeneratedToken()
    {
        var dbContext = GetInMemoryDbContext();
        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.GenerateDevAccessToken(7)).Returns("dev-token");

        var service = new JwtService(jwtProviderMock.Object, dbContext);

        var result = service.GenerateDevAccessToken(7);

        result.Should().Be("dev-token");
    }
}
