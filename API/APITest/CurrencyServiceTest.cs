using API.Models;
using API.Services.Currency;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class CurrencyServiceTest : BaseTest
{
    [Fact]
    public async Task GetAllCurrencies_ReturnsAllCurrencies()
    {
        var dbContext = GetInMemoryDbContext();

        dbContext.Currencies.AddRange(
            new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m },
            new Currency { Id = 2, Name = "EUR", UsdExchangeRate = 1.1m }
        );
        await dbContext.SaveChangesAsync();

        var notificationContext = new NotificationContext();
        var service = new CurrencyService(dbContext, notificationContext);

        var result = await service.GetAllCurrencies();

        result.Should().HaveCount(2);
        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrencyById_ReturnsCurrency_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Currencies.Add(new Currency { Id = 10, Name = "UAH", UsdExchangeRate = 0.025m });
        await dbContext.SaveChangesAsync();

        var notificationContext = new NotificationContext();
        var service = new CurrencyService(dbContext, notificationContext);

        var result = await service.GetCurrencyById(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.Name.Should().Be("UAH");
        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrencyById_ReturnsNull_WhenNotExists()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new CurrencyService(dbContext, notificationContext);

        var result = await service.GetCurrencyById(999);

        result.Should().BeNull();
        notificationContext.HasNotifications.Should().BeTrue();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }
}
