using API.Dtos;
using API.Services;
using API.Services.User;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace APITest;

public class UserLoggingServiceTest
{
    [Fact]
    public async Task CreateUser_DelegatesToInnerService()
    {
        var inner = new Mock<IUserService>();
        inner.Setup(i => i.CreateUser("u", "e@mail.com", "p"))
            .ReturnsAsync(new AuthUserDto { AccessToken = "a", RefreshToken = "r" });

        var loggerMock = new Mock<ILogger<UserLoggingService>>();
        var service = new UserLoggingService(inner.Object, loggerMock.Object);

        var result = await service.CreateUser("u", "e@mail.com", "p");

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("a");
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("User created with email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutUser_DelegatesToInnerService()
    {
        var inner = new Mock<IUserService>();
        inner.Setup(i => i.LogoutUser("token")).ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<UserLoggingService>>();
        var service = new UserLoggingService(inner.Object, loggerMock.Object);

        var result = await service.LogoutUser("token");

        result.Should().BeTrue();
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("User logged out")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginUser_DelegatesToInnerService()
    {
        var inner = new Mock<IUserService>();
        inner.Setup(i => i.LoginUser("e@mail.com", "p"))
            .ReturnsAsync(new AuthUserDto { AccessToken = "a", RefreshToken = "r" });

        var loggerMock = new Mock<ILogger<UserLoggingService>>();
        var service = new UserLoggingService(inner.Object, loggerMock.Object);

        var result = await service.LoginUser("e@mail.com", "p");

        result.Should().NotBeNull();
        result!.RefreshToken.Should().Be("r");
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("User logged in with email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task IsEmailAvailable_DelegatesToInnerService()
    {
        var inner = new Mock<IUserService>();
        inner.Setup(i => i.IsEmailAvailable("email@mail.com")).ReturnsAsync(true);

        var service = new UserLoggingService(inner.Object, Mock.Of<ILogger<UserLoggingService>>());

        var result = await service.IsEmailAvailable("email@mail.com");

        result.Should().BeTrue();
    }
}
