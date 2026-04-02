using API.Services.Jwt;
using API.Services.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace APITest;

public class JwtLoggingServiceTest
{
    [Fact]
    public async Task RefreshToken_ReturnsInnerResult()
    {
        var innerMock = new Mock<IJwtService>();
        innerMock.Setup(i => i.RefreshToken("r")).ReturnsAsync("a");

        var httpContextAccessor = new HttpContextAccessor();
        var loggerMock = new Mock<ILogger<JwtLoggingService>>();

        var service = new JwtLoggingService(innerMock.Object, loggerMock.Object, httpContextAccessor);

        var result = await service.RefreshToken("r");

        result.Should().Be("a");
    }

    [Fact]
    public async Task RefreshToken_ReturnsNull_WhenInnerReturnsNull()
    {
        var innerMock = new Mock<IJwtService>();
        innerMock.Setup(i => i.RefreshToken("invalid")).ReturnsAsync((string?)null);

        var loggerMock = new Mock<ILogger<JwtLoggingService>>();
        var service = new JwtLoggingService(innerMock.Object, loggerMock.Object, new HttpContextAccessor());

        var result = await service.RefreshToken("invalid");

        result.Should().BeNull();
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to refresh access token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GenerateDevAccessToken_ReturnsInnerToken()
    {
        var innerMock = new Mock<IJwtService>();
        innerMock.Setup(i => i.GenerateDevAccessToken(1)).Returns("dev");

        var loggerMock = new Mock<ILogger<JwtLoggingService>>();
        var service = new JwtLoggingService(innerMock.Object, loggerMock.Object, new HttpContextAccessor());

        var result = service.GenerateDevAccessToken(1);

        result.Should().Be("dev");
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Generated dev access token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
