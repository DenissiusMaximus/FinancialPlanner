using API.Models;
using API.Services;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class TransactionTypeServiceTest : BaseTest
{
    [Fact]
    public async Task GetTransactionTypes_ReturnsAll()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.TransactionTypes.AddRange(
            new TransactionType { Id = 1, Name = "Expense" },
            new TransactionType { Id = 2, Name = "Income" }
        );
        await dbContext.SaveChangesAsync();

        var notificationContext = new NotificationContext();
        var service = new TransactionTypeService(dbContext, notificationContext);

        var result = await service.GetTransactionTypes();

        result.Should().HaveCount(2);
        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task GetTransactionType_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new TransactionTypeService(dbContext, notificationContext);

        var result = await service.GetTransactionType(404);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetTransactionType_ReturnsType_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.TransactionTypes.Add(new TransactionType { Id = 5, Name = "Transfer" });
        await dbContext.SaveChangesAsync();

        var service = new TransactionTypeService(dbContext, new NotificationContext());

        var result = await service.GetTransactionType(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }
}
