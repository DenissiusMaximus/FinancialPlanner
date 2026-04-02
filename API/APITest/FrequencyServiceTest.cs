using API.Inputs;
using API.Models;
using API.Services.Frequency;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class FrequencyServiceTest : BaseTest
{
    [Fact]
    public async Task CreateFrequency_ReturnsNull_WhenIntervalUnitNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new FrequencyService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.CreateFrequency(new CreateFrequencyInput { Name = "Weekly", IntervalUnitId = 999, IntervalValue = 1 });

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateFrequency_ReturnsCreatedFrequency()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        await dbContext.SaveChangesAsync();

        var service = new FrequencyService(dbContext, new NotificationContext(), GetMockUserContext(1));

        var result = await service.CreateFrequency(new CreateFrequencyInput { Name = "Weekly", IntervalUnitId = 1, IntervalValue = 1 });

        result.Should().NotBeNull();
        result!.Name.Should().Be("Weekly");
        result.UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetFrequencies_ReturnsUserAndSharedFrequencies()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.AddRange(
            new Frequency { Id = 1, Name = "Shared", IntervalValue = 1, IntervalUnitId = 1, UserId = null },
            new Frequency { Id = 2, Name = "Mine", IntervalValue = 2, IntervalUnitId = 1, UserId = 1 },
            new Frequency { Id = 3, Name = "Other", IntervalValue = 3, IntervalUnitId = 1, UserId = 2 }
        );
        await dbContext.SaveChangesAsync();

        var service = new FrequencyService(dbContext, new NotificationContext(), GetMockUserContext(1));

        var result = await service.GetFrequencies();

        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task GetFrequency_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new FrequencyService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.GetFrequency(777);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetFrequency_ReturnsFrequency_WhenExistsForUser()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.Add(new Frequency { Id = 10, Name = "UserFreq", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 });
        await dbContext.SaveChangesAsync();

        var service = new FrequencyService(dbContext, new NotificationContext(), GetMockUserContext(1));

        var result = await service.GetFrequency(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }

    [Fact]
    public async Task DeleteFrequency_ReturnsFalse_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new FrequencyService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.DeleteFrequency(777);

        result.Should().BeFalse();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteFrequency_ReturnsTrue_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Frequencies.Add(new Frequency { Id = 12, Name = "ToDelete", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 });
        await dbContext.SaveChangesAsync();

        var service = new FrequencyService(dbContext, new NotificationContext(), GetMockUserContext(1));

        var result = await service.DeleteFrequency(12);

        result.Should().BeTrue();
        dbContext.Frequencies.Any(f => f.Id == 12).Should().BeFalse();
    }

    [Fact]
    public async Task GetUserFrequencies_ReturnsOnlyUsersFrequencies()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.AddRange(
            new Frequency { Id = 20, Name = "Mine", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 },
            new Frequency { Id = 21, Name = "Other", IntervalValue = 1, IntervalUnitId = 1, UserId = 2 },
            new Frequency { Id = 22, Name = "Shared", IntervalValue = 1, IntervalUnitId = 1, UserId = null }
        );
        await dbContext.SaveChangesAsync();

        var service = new FrequencyService(dbContext, new NotificationContext(), GetMockUserContext(1));

        var result = await service.GetUserFrequencies();

        result.Should().ContainSingle();
        result.First().Id.Should().Be(20);
    }

    [Fact]
    public async Task UpdateFrequency_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new FrequencyService(dbContext, notificationContext, GetMockUserContext(1));

        var result = await service.UpdateFrequency(new UpdateFrequencyInput { Name = "new" }, 404);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateFrequency_ReturnsUpdatedFrequency()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.Add(new Frequency { Id = 11, Name = "Old", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 });
        await dbContext.SaveChangesAsync();

        var service = new FrequencyService(dbContext, new NotificationContext(), GetMockUserContext(1));

        var result = await service.UpdateFrequency(new UpdateFrequencyInput { Name = "New" }, 11);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New");
    }
}
