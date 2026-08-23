using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Features.Users.Commands.LoginUser;
using FinancialPlanner.Application.Features.Users.Commands.LogoutUser;
using FinancialPlanner.Application.Features.Users.Commands.RegisterUser;
using FinancialPlanner.Application.Features.Users.Queries.GetCurrentUser;
using FinancialPlanner.Application.Features.Users.Queries.IsEmailAvailable;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;
using Moq;

namespace FinancialPlanner.Application.Tests.Features.Users;

public class UserHandlersTest : BaseTest
{
    [Fact]
    public async Task RegisterUser_ReturnsTokens_WhenEmailIsUnique()
    {
        var dbContext = GetInMemoryDbContext();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.HashPassword("password1")).Returns("hashed");

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.GenerateAccessToken(It.IsAny<int>())).Returns("access");
        jwtProviderMock.Setup(j => j.GenerateRefreshToken(It.IsAny<int>())).Returns("refresh");

        var handler = new RegisterUserCommandHandler(
            new RegisterUserCommandValidator(),
            new UserRepository(dbContext),
            new UnitOfWork(dbContext),
            passwordHasherMock.Object,
            jwtProviderMock.Object);

        var result = await handler.HandleAsync(new RegisterUserCommand("John", "john@mail.com", "password1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
        dbContext.Users.Should().ContainSingle(u => u.Email == "john@mail.com");
    }

    [Fact]
    public async Task RegisterUser_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 1, Name = "Existing", Email = "dup@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed");

        var handler = new RegisterUserCommandHandler(
            new RegisterUserCommandValidator(),
            new UserRepository(dbContext),
            new UnitOfWork(dbContext),
            passwordHasherMock.Object,
            Mock.Of<IJwtProvider>());

        var result = await handler.HandleAsync(new RegisterUserCommand("John", "dup@mail.com", "password1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.EmailAlreadyUsed("dup@mail.com").Code);
    }

    [Fact]
    public async Task LoginUser_ReturnsTokens_WhenCredentialsAreValid()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 5, Name = "User", Email = "u@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.VerifyPassword("password1", "hash")).Returns(true);

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.GenerateAccessToken(5)).Returns("access");
        jwtProviderMock.Setup(j => j.GenerateRefreshToken(5)).Returns("refresh");

        var handler = new LoginUserCommandHandler(new LoginUserCommandValidator(), new UserRepository(dbContext), passwordHasherMock.Object, jwtProviderMock.Object);

        var result = await handler.HandleAsync(new LoginUserCommand("u@mail.com", "password1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task LoginUser_ReturnsInvalidCredentials_WhenUserNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new LoginUserCommandHandler(new LoginUserCommandValidator(), new UserRepository(dbContext), Mock.Of<IPasswordHasher>(), Mock.Of<IJwtProvider>());

        var result = await handler.HandleAsync(new LoginUserCommand("missing@mail.com", "password1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task LoginUser_ReturnsInvalidCredentials_WhenPasswordIsInvalid()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 5, Name = "User", Email = "u@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.VerifyPassword("bad-pass1", "hash")).Returns(false);

        var handler = new LoginUserCommandHandler(new LoginUserCommandValidator(), new UserRepository(dbContext), passwordHasherMock.Object, Mock.Of<IJwtProvider>());

        var result = await handler.HandleAsync(new LoginUserCommand("u@mail.com", "bad-pass1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task IsEmailAvailable_ReturnsFalse_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.Add(new User { Id = 1, Name = "User", Email = "exists@mail.com", PasswordHash = "hash" });
        await dbContext.SaveChangesAsync();

        var handler = new IsEmailAvailableQueryHandler(new UserRepository(dbContext));

        var result = await handler.HandleAsync(new IsEmailAvailableQuery("exists@mail.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailAvailable_ReturnsTrue_WhenNotExists()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new IsEmailAvailableQueryHandler(new UserRepository(dbContext));

        var result = await handler.HandleAsync(new IsEmailAvailableQuery("free@mail.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsCurrentUser()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Users.AddRange(
            new User { Id = 1, Name = "User1", Email = "u1@mail.com", PasswordHash = "h1" },
            new User { Id = 2, Name = "User2", Email = "u2@mail.com", PasswordHash = "h2" });
        await dbContext.SaveChangesAsync();

        var handler = new GetCurrentUserQueryHandler(new UserRepository(dbContext), GetMockUserContext(2), GetMapper());

        var result = await handler.HandleAsync(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(2);
        result.Value.Email.Should().Be("u2@mail.com");
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsNotFound_WhenUserMissing()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetCurrentUserQueryHandler(new UserRepository(dbContext), GetMockUserContext(999), GetMapper());

        var result = await handler.HandleAsync(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task LogoutUser_BlacklistsToken_WhenRefreshTokenIsValid()
    {
        var dbContext = GetInMemoryDbContext();

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.ValidateRefreshToken("refresh-token"))
            .Returns(new JwtValidationResult(1, "jti-1", DateTime.UtcNow.AddDays(1)));

        var handler = new LogoutUserCommandHandler(jwtProviderMock.Object, new BlacklistedTokenRepository(dbContext), new UnitOfWork(dbContext));

        var result = await handler.HandleAsync(new LogoutUserCommand("refresh-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.BlacklistedTokens.Should().ContainSingle(t => t.Jti == "jti-1");
    }

    [Fact]
    public async Task LogoutUser_Fails_WhenRefreshTokenIsInvalid()
    {
        var dbContext = GetInMemoryDbContext();

        var jwtProviderMock = new Mock<IJwtProvider>();
        jwtProviderMock.Setup(j => j.ValidateRefreshToken("bad-token")).Returns((JwtValidationResult?)null);

        var handler = new LogoutUserCommandHandler(jwtProviderMock.Object, new BlacklistedTokenRepository(dbContext), new UnitOfWork(dbContext));

        var result = await handler.HandleAsync(new LogoutUserCommand("bad-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.LogoutFailed.Code);
    }
}
