using API.Dtos;
using API.Inputs;
using API.Models;
using API.Services.Source;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class SourceServiceTest : BaseTest
{
    [Fact]
    public async Task GetSources_ReturnsOnlyCurrentUserSources()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.AddRange(
            new Source { Id = 1, Name = "S1", Amount = 100, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false },
            new Source { Id = 2, Name = "S2", Amount = 200, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false },
            new Source { Id = 3, Name = "Other", Amount = 300, UserId = 2, CurrencyId = 1, Currency = currency, IsArchived = false }
        );
        await dbContext.SaveChangesAsync();

        var notificationContext = new NotificationContext();
        var service = new SourceService(dbContext, notificationContext, GetMockUserContext(userId));

        var result = await service.GetSources();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.UserId == userId);
    }

    [Fact]
    public async Task GetSourceById_ReturnsSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 7, Name = "Wallet", Amount = 500, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var service = new SourceService(dbContext, new NotificationContext(), GetMockUserContext(userId));

        var result = await service.GetSourceById(7);

        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
    }

    [Fact]
    public async Task CreateSource_ReturnsNull_WhenCurrencyNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new SourceService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.CreateSource(new CreateSourceInput { Name = "Cash", Amount = 100, CurrencyId = 404 });

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateSource_ReturnsCreatedSource_WhenCurrencyExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });
        await dbContext.SaveChangesAsync();

        var service = new SourceService(dbContext, new NotificationContext(), GetMockUserContext(1));

        var result = await service.CreateSource(new CreateSourceInput { Name = "Cash", Amount = 100, CurrencyId = 1 });

        result.Should().NotBeNull();
        result!.Name.Should().Be("Cash");
        result.UserId.Should().Be(1);
    }

    [Fact]
    public async Task ArchiveSource_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new SourceService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.ArchiveSource(999);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ArchiveSource_ReturnsUpdatedSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 8, Name = "Wallet", Amount = 500, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var service = new SourceService(dbContext, new NotificationContext(), GetMockUserContext(userId));

        var result = await service.ArchiveSource(8);

        result.Should().NotBeNull();
        result!.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task UnArchiveSource_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new SourceService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.UnArchiveSource(999);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateSource_ReturnsUpdatedSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 4, Name = "Old", Amount = 99, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var service = new SourceService(dbContext, new NotificationContext(), GetMockUserContext(userId));

        var result = await service.UpdateSource(4, new UpdateSourceInput { Name = "New" });

        result.Should().NotBeNull();
        result!.Name.Should().Be("New");
    }

    [Fact]
    public async Task UpdateSource_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new SourceService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.UpdateSource(999, new UpdateSourceInput { Name = "New" });

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UnArchiveSource_ReturnsUpdatedSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 9, Name = "Wallet", Amount = 500, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = true });
        await dbContext.SaveChangesAsync();

        var service = new SourceService(dbContext, new NotificationContext(), GetMockUserContext(userId));

        var result = await service.UnArchiveSource(9);

        result.Should().NotBeNull();
        result!.IsArchived.Should().BeFalse();
    }
}
