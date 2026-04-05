using API.Models;
using API.Services;
using API.Services.PlannedTransaction;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class PlannedTransactionServiceTest : BaseTest
{
    [Fact]
    public async Task CreatePlannedTransaction_ReturnsCreatedTransaction()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        var service = new PlannedTransactionService(dbContext, GetMockUserContext(userId), new NotificationContext());

        var result = await service.CreatePlannedTransaction(new CreatePlannedTransactionInput
        {
            Name = "Rent",
            Amount = 1000,
            StartDate = new DateOnly(2026, 1, 1),
            CurrencyId = 1,
            TransactionTypeId = 1,
            CategoryId = 1,
            SourceId = 1,
            FrequencyId = 1
        });

        result.Should().NotBeNull();
        result!.Name.Should().Be("Rent");
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetPlannedTransactionById_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new PlannedTransactionService(dbContext, GetMockUserContext(1), notificationContext);

        var result = await service.GetPlannedTransactionById(999);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetPlannedTransactionById_ReturnsTransaction_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.Add(new PlannedTransaction
        {
            Id = 3,
            Name = "Existing",
            Amount = 100,
            StartDate = new DateOnly(2026, 1, 1),
            UserId = userId,
            CurrencyId = 1,
            TransactionTypeId = 1,
            SourceId = 1,
            FrequencyId = 1
        });
        await dbContext.SaveChangesAsync();

        var service = new PlannedTransactionService(dbContext, GetMockUserContext(userId), new NotificationContext());

        var result = await service.GetPlannedTransactionById(3);

        result.Should().NotBeNull();
        result!.Id.Should().Be(3);
    }

    [Fact]
    public async Task GetUsersPlannedTransactions_ReturnsOnlyCurrentUsers()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedDependencies(dbContext, 1);
        await SeedDependencies(dbContext, 2);

        dbContext.PlannedTransactions.AddRange(
            new PlannedTransaction { Id = 1, Name = "P1", Amount = 100, StartDate = new DateOnly(2026, 1, 1), UserId = 1, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 },
            new PlannedTransaction { Id = 2, Name = "P2", Amount = 200, StartDate = new DateOnly(2026, 1, 2), UserId = 2, CurrencyId = 1, TransactionTypeId = 1, SourceId = 2, FrequencyId = 2 }
        );
        await dbContext.SaveChangesAsync();

        var service = new PlannedTransactionService(dbContext, GetMockUserContext(1), new NotificationContext());

        var result = await service.GetUsersPlannedTransactions(new());

        result.Data.Should().ContainSingle();
        result.Data.First().UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetUsersPlannedTransactions_AppliesLimitAndOffset()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.AddRange(
            new PlannedTransaction { Id = 11, Name = "A", Amount = 100, StartDate = new DateOnly(2026, 1, 1), UserId = userId, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 },
            new PlannedTransaction { Id = 12, Name = "B", Amount = 100, StartDate = new DateOnly(2026, 1, 1), UserId = userId, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 },
            new PlannedTransaction { Id = 13, Name = "C", Amount = 100, StartDate = new DateOnly(2026, 1, 1), UserId = userId, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 }
        );
        await dbContext.SaveChangesAsync();

        var service = new PlannedTransactionService(dbContext, GetMockUserContext(userId), new NotificationContext());

        var result = await service.GetUsersPlannedTransactions(new GetUserPlannedTransactionsInput { Limit = 1, Offset = 1 });

        result.Data.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdatePlannedTransaction_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new PlannedTransactionService(dbContext, GetMockUserContext(1), notificationContext);

        var result = await service.UpdatePlannedTransaction(999, new UpdatePlannedTransactionInput { Name = "Updated" });

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdatePlannedTransaction_ReturnsUpdatedTransaction_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.Add(new PlannedTransaction
        {
            Id = 5,
            Name = "Old Name",
            Amount = 500,
            StartDate = new DateOnly(2026, 1, 1),
            UserId = userId,
            CurrencyId = 1,
            TransactionTypeId = 1,
            SourceId = 1,
            FrequencyId = 1
        });
        await dbContext.SaveChangesAsync();

        var service = new PlannedTransactionService(dbContext, GetMockUserContext(userId), new NotificationContext());

        var result = await service.UpdatePlannedTransaction(5, new UpdatePlannedTransactionInput { Name = "New Name", Amount = 700 });

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Amount.Should().Be(700);
    }

    [Fact]
    public async Task DeletePlannedTransaction_ReturnsFalse_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();
        var service = new PlannedTransactionService(dbContext, GetMockUserContext(1), notificationContext);

        var result = await service.DeletePlannedTransaction(999);

        result.Should().BeFalse();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeletePlannedTransaction_ReturnsTrue_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.Add(new PlannedTransaction
        {
            Id = 6,
            Name = "To Delete",
            Amount = 300,
            StartDate = new DateOnly(2026, 1, 1),
            UserId = userId,
            CurrencyId = 1,
            TransactionTypeId = 1,
            SourceId = 1,
            FrequencyId = 1
        });
        await dbContext.SaveChangesAsync();

        var service = new PlannedTransactionService(dbContext, GetMockUserContext(userId), new NotificationContext());

        var result = await service.DeletePlannedTransaction(6);

        result.Should().BeTrue();
        dbContext.PlannedTransactions.Any(t => t.Id == 6).Should().BeFalse();
    }

    private static async Task SeedDependencies(API.AppDbContext dbContext, int userId)
    {
        if (!dbContext.Currencies.Any(c => c.Id == 1))
            dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });

        if (!dbContext.TransactionTypes.Any(t => t.Id == 1))
            dbContext.TransactionTypes.Add(new TransactionType { Id = 1, Name = "Expense" });

        if (!dbContext.IntervalUnits.Any(i => i.Id == userId))
            dbContext.IntervalUnits.Add(new IntervalUnit { Id = userId, Name = $"Unit{userId}" });

        if (!dbContext.Frequencies.Any(f => f.Id == userId))
            dbContext.Frequencies.Add(new Frequency { Id = userId, Name = $"Freq{userId}", IntervalValue = 1, IntervalUnitId = userId, UserId = userId });

        if (!dbContext.Sources.Any(s => s.Id == userId))
            dbContext.Sources.Add(new Source { Id = userId, Name = $"Source{userId}", Amount = 1000, UserId = userId, CurrencyId = 1, IsArchived = false });

        if (!dbContext.Categories.Any(c => c.Id == userId))
            dbContext.Categories.Add(new Category { Id = userId, Name = $"Category{userId}", UserId = userId });

        await dbContext.SaveChangesAsync();
    }
}
