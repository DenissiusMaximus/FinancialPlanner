using API.Models;
using API.Services.User;
using API.Utils;
using API.Utils.JwtProvider;
using FluentAssertions;
using Moq;

namespace APITest;

public class UserServiceTest : BaseTest
{
    [Fact]
    public async Task CreateUser_ReturnsTokens_WhenEmailIsUnique()
    {
        var dbContext = GetInMemoryDbContext();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.HashPassword("pass")).Returns("hashed");

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.GenerateAccessToken(It.IsAny<int>())).Returns("access");
        jwtProviderMock.Setup(j => j.GenerateRefreshToken(It.IsAny<int>())).Returns("refresh");

        var service = new UserService(dbContext, passwordHasherMock.Object, jwtProviderMock.Object, GetMockUserContext(1));

        var result = await service.CreateUser("John", "john@mail.com", "pass");

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");
        dbContext.Users.Should().ContainSingle(u => u.Email == "john@mail.com");
    }

    [Fact]
    public async Task CreateUser_ReturnsNull_WhenEmailAlreadyExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 1, Name = "Existing", Email = "dup@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed");

        var jwtProviderMock = new Mock<IJwtProvider>();

        var service = new UserService(dbContext, passwordHasherMock.Object, jwtProviderMock.Object, GetMockUserContext(1));

        var result = await service.CreateUser("John", "dup@mail.com", "pass");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginUser_ReturnsTokens_WhenCredentialsAreValid()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 5, Name = "User", Email = "u@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.VerifyPassword("pass", "hash")).Returns(true);

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.GenerateAccessToken(5)).Returns("access");
        jwtProviderMock.Setup(j => j.GenerateRefreshToken(5)).Returns("refresh");

        var service = new UserService(dbContext, passwordHasherMock.Object, jwtProviderMock.Object, GetMockUserContext(5));

        var result = await service.LoginUser("u@mail.com", "pass");

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task LoginUser_ReturnsNull_WhenUserNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var service = new UserService(dbContext, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtProvider>(), GetMockUserContext(1));

        var result = await service.LoginUser("missing@mail.com", "pass");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginUser_ReturnsNull_WhenPasswordIsInvalid()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 5, Name = "User", Email = "u@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.VerifyPassword("bad-pass", "hash")).Returns(false);

        var service = new UserService(dbContext, passwordHasherMock.Object, Mock.Of<IJwtProvider>(), GetMockUserContext(5));

        var result = await service.LoginUser("u@mail.com", "bad-pass");

        result.Should().BeNull();
    }

    [Fact]
    public async Task IsEmailAvailable_ReturnsFalse_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 1, Name = "User", Email = "exists@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var service = new UserService(dbContext, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtProvider>(), GetMockUserContext(1));

        var result = await service.IsEmailAvailable("exists@mail.com");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailAvailable_ReturnsTrue_WhenNotExists()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new UserService(dbContext, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtProvider>(), GetMockUserContext(1));

        var result = await service.IsEmailAvailable("free@mail.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsCurrentUser()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.AddRange(
            new User { Id = 1, Name = "User1", Email = "u1@mail.com", PasswordHash = "h1" },
            new User { Id = 2, Name = "User2", Email = "u2@mail.com", PasswordHash = "h2" }
        );
        await dbContext.SaveChangesAsync();

        var service = new UserService(dbContext, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtProvider>(), GetMockUserContext(2));

        var result = await service.GetCurrentUser();

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Email.Should().Be("u2@mail.com");
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsNull_WhenUserNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var service = new UserService(dbContext, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtProvider>(), GetMockUserContext(999));

        var result = await service.GetCurrentUser();

        result.Should().BeNull();
    }

    [Fact]
    public async Task LogoutUser_ReturnsInnerProviderResult()
    {
        var dbContext = GetInMemoryDbContext();

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.AddTokenToBlacklistAsync("refresh-token")).ReturnsAsync(true);

        var service = new UserService(dbContext, Mock.Of<IPasswordHasher>(), jwtProviderMock.Object, GetMockUserContext(1));

        var result = await service.LogoutUser("refresh-token");

        result.Should().BeTrue();
        jwtProviderMock.Verify(j => j.AddTokenToBlacklistAsync("refresh-token"), Times.Once);
    }
}
