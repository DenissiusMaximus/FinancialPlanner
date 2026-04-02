using API.Models;
using API.Services.InteralUnit;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class IntervalUnitServiceTest : BaseTest
{
    [Fact]
    public async Task GetIntervalUnits_ReturnsAllUnits()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.AddRange(
            new IntervalUnit { Id = 1, Name = "Day" },
            new IntervalUnit { Id = 2, Name = "Month" }
        );
        await dbContext.SaveChangesAsync();

        var notificationContext = new NotificationContext();
        var service = new IntervalUnitService(dbContext, notificationContext);

        var result = await service.GetIntervalUnits();

        result.Should().HaveCount(2);
        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task GetIntervalUnitById_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new IntervalUnitService(dbContext, notificationContext);

        var result = await service.GetIntervalUnitById(123);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetIntervalUnitById_ReturnsUnit_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 10, Name = "Week" });
        await dbContext.SaveChangesAsync();

        var service = new IntervalUnitService(dbContext, new NotificationContext());

        var result = await service.GetIntervalUnitById(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }
}
