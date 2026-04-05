using API.Domain.BalanceManagement;
using API.Inputs;
using API.Models;
using API.Services;
using API.Services.Transaction;
using API.Utils.Notification;
using FluentAssertions;
using Moq;

namespace APITest;

public class TransactionServiceTest : BaseTest
{
    [Fact]
    public async Task CreateTransaction_Transfer_UpdatesBothSourceBalances_WhenUsingRealBalanceService()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedTransactionDependencies(dbContext, userId);

        dbContext.Sources.Add(new Source { Id = 11, Name = "Destination", Amount = 200, UserId = userId, CurrencyId = 1, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var notificationContext = new NotificationContext();
        var balanceService = new BalanceManagementService(dbContext, notificationContext);
        var service = new TransactionService(dbContext, GetMockUserContext(userId), notificationContext, balanceService);

        var result = await service.CreateTransaction(new CreateTransactionInput
        {
            Amount = 100,
            Date = new DateOnly(2026, 1, 15),
            SourceId = 1,
            DestinationSourceId = 11,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Transfer
        });

        var source = await dbContext.Sources.FindAsync(1);
        var destination = await dbContext.Sources.FindAsync(11);

        result.Should().NotBeNull();
        source!.Amount.Should().Be(900);
        destination!.Amount.Should().Be(300);
        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTransaction_Transfer_CallsUpdateAmountsAndReturnsCreatedTransaction()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Sources.Add(new Source { Id = 10, Name = "Dest", Amount = 500, UserId = 1, CurrencyId = 1, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        balanceManagementServiceMock
            .Setup(b => b.UpdateAmounts(It.IsAny<Transaction>(), It.IsAny<Source>(), 1))
            .ReturnsAsync(true);

        var notificationContext = new NotificationContext();
        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, balanceManagementServiceMock.Object);

        var input = new CreateTransactionInput
        {
            Amount = 100,
            Date = new DateOnly(2026, 1, 10),
            SourceId = 1,
            DestinationSourceId = 10,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Transfer,
            Comment = "Transfer to savings"
        };

        var result = await service.CreateTransaction(input);

        result.Should().NotBeNull();
        result!.TransactionType.Id.Should().Be((int)TransactionTypeEnum.Transfer);
        result.Source.Id.Should().Be(1);
        result.DestinationSource!.Id.Should().Be(10);
        balanceManagementServiceMock.Verify(
            b => b.UpdateAmounts(
                It.Is<Transaction>(t => t.TransactionTypeId == (int)TransactionTypeEnum.Transfer && t.DestinationSourceId == 10),
                It.Is<Source>(s => s.Id == 1),
                1),
            Times.Once);
        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTransaction_ReturnsNull_WhenSourceNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });
        dbContext.TransactionTypes.Add(new TransactionType { Id = 1, Name = "Expense" });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        var notificationContext = new NotificationContext();
        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, balanceManagementServiceMock.Object);

        var result = await service.CreateTransaction(new CreateTransactionInput
        {
            Amount = 50,
            Date = new DateOnly(2026, 2, 1),
            SourceId = 999,
            CurrencyId = 1,
            TransactionTypeId = 1
        });

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
        balanceManagementServiceMock.Verify(b => b.UpdateAmounts(It.IsAny<Transaction>(), It.IsAny<Source>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsNull_WhenBalanceUpdateFails()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        balanceManagementServiceMock
            .Setup(b => b.UpdateAmounts(It.IsAny<Transaction>(), It.IsAny<Source>(), 1))
            .ReturnsAsync(false);

        var service = new TransactionService(dbContext, GetMockUserContext(1), new NotificationContext(), balanceManagementServiceMock.Object);

        var result = await service.CreateTransaction(new CreateTransactionInput
        {
            Amount = 50,
            Date = new DateOnly(2026, 2, 1),
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = 1
        });

        result.Should().BeNull();
        dbContext.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsFalse_ForAdjustmentTransaction()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.Add(new Transaction
        {
            Id = 20,
            Amount = 100,
            Date = new DateOnly(2026, 1, 1),
            UserId = 1,
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Adjustment
        });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        var notificationContext = new NotificationContext();
        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, balanceManagementServiceMock.Object);

        var result = await service.DeleteTransaction(20);

        result.Should().BeFalse();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.BadRequest);
        balanceManagementServiceMock.Verify(b => b.ResetTransaction(It.IsAny<Transaction>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsFalse_WhenTransactionNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        var notificationContext = new NotificationContext();
        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, Mock.Of<IBalanceManagementService>());

        var result = await service.DeleteTransaction(999);

        result.Should().BeFalse();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsTrue_WhenResetSucceeds()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.Add(new Transaction
        {
            Id = 21,
            Amount = 100,
            Date = new DateOnly(2026, 1, 1),
            UserId = 1,
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Expense
        });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        balanceManagementServiceMock.Setup(b => b.ResetTransaction(It.IsAny<Transaction>(), 1)).ReturnsAsync(true);

        var service = new TransactionService(dbContext, GetMockUserContext(1), new NotificationContext(), balanceManagementServiceMock.Object);

        var result = await service.DeleteTransaction(21);

        result.Should().BeTrue();
        dbContext.Transactions.Any(t => t.Id == 21).Should().BeFalse();
        balanceManagementServiceMock.Verify(b => b.ResetTransaction(It.Is<Transaction>(t => t.Id == 21), 1), Times.Once);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsFalse_WhenResetFails()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.Add(new Transaction
        {
            Id = 22,
            Amount = 120,
            Date = new DateOnly(2026, 1, 1),
            UserId = 1,
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Expense
        });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        balanceManagementServiceMock.Setup(b => b.ResetTransaction(It.IsAny<Transaction>(), 1)).ReturnsAsync(false);

        var service = new TransactionService(dbContext, GetMockUserContext(1), new NotificationContext(), balanceManagementServiceMock.Object);

        var result = await service.DeleteTransaction(22);

        result.Should().BeFalse();
        dbContext.Transactions.Should().ContainSingle(t => t.Id == 22);
    }

    [Fact]
    public async Task UpdateTransaction_CallsResetAndUpdateAmounts_WhenAmountChanged()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.Add(new Transaction
        {
            Id = 30,
            Amount = 100,
            Date = new DateOnly(2026, 1, 1),
            UserId = 1,
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Expense
        });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        balanceManagementServiceMock.Setup(b => b.ResetTransaction(It.IsAny<Transaction>(), 1)).ReturnsAsync(true);
        balanceManagementServiceMock.Setup(b => b.UpdateAmounts(It.IsAny<Transaction>(), It.IsAny<Source>(), 1)).ReturnsAsync(true);

        var notificationContext = new NotificationContext();
        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, balanceManagementServiceMock.Object);

        var result = await service.UpdateTransaction(30, new UpdateTransactionInput { Amount = 250 });

        result.Should().NotBeNull();
        result!.Amount.Should().Be(250);
        balanceManagementServiceMock.Verify(b => b.ResetTransaction(It.Is<Transaction>(t => t.Id == 30), 1), Times.Once);
        balanceManagementServiceMock.Verify(b => b.UpdateAmounts(It.Is<Transaction>(t => t.Id == 30 && t.Amount == 250), It.Is<Source>(s => s.Id == 1), 1), Times.Once);
        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTransaction_DoesNotRecalculateBalances_WhenOnlyCommentChanged()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.Add(new Transaction
        {
            Id = 32,
            Amount = 100,
            Date = new DateOnly(2026, 1, 1),
            UserId = 1,
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Expense,
            Comment = "old"
        });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        var service = new TransactionService(dbContext, GetMockUserContext(1), new NotificationContext(), balanceManagementServiceMock.Object);

        var result = await service.UpdateTransaction(32, new UpdateTransactionInput { Comment = "new" });

        result.Should().NotBeNull();
        result!.Comment.Should().Be("new");
        balanceManagementServiceMock.Verify(b => b.ResetTransaction(It.IsAny<Transaction>(), It.IsAny<int>()), Times.Never);
        balanceManagementServiceMock.Verify(b => b.UpdateAmounts(It.IsAny<Transaction>(), It.IsAny<Source>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsNull_ForAdjustmentTransaction()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.Add(new Transaction
        {
            Id = 31,
            Amount = 100,
            Date = new DateOnly(2026, 1, 1),
            UserId = 1,
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = (int)TransactionTypeEnum.Adjustment
        });
        await dbContext.SaveChangesAsync();

        var balanceManagementServiceMock = new Mock<IBalanceManagementService>();
        var notificationContext = new NotificationContext();
        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, balanceManagementServiceMock.Object);

        var result = await service.UpdateTransaction(31, new UpdateTransactionInput { Amount = 123 });

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.BadRequest);
        balanceManagementServiceMock.Verify(b => b.ResetTransaction(It.IsAny<Transaction>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsNull_WhenTransactionNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        var notificationContext = new NotificationContext();
        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, Mock.Of<IBalanceManagementService>());

        var result = await service.UpdateTransaction(999, new UpdateTransactionInput { Amount = 123 });

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle()
            .Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsTransaction_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.Add(new Transaction
        {
            Id = 1,
            Amount = 50,
            Date = new DateOnly(2026, 1, 1),
            UserId = 1,
            SourceId = 1,
            CurrencyId = 1,
            TransactionTypeId = 1
        });
        await dbContext.SaveChangesAsync();

        var service = new TransactionService(dbContext, GetMockUserContext(1), new NotificationContext(), Mock.Of<IBalanceManagementService>());

        var result = await service.GetTransactionById(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsNull_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var notificationContext = new NotificationContext();

        var service = new TransactionService(dbContext, GetMockUserContext(1), notificationContext, Mock.Of<IBalanceManagementService>());

        var result = await service.GetTransactionById(999);

        result.Should().BeNull();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetUsersTransactions_FiltersByDateAndSort()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Transactions.AddRange(
            new Transaction { Id = 1, Amount = 100, Date = new DateOnly(2026, 1, 2), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 },
            new Transaction { Id = 2, Amount = 300, Date = new DateOnly(2026, 1, 3), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 },
            new Transaction { Id = 3, Amount = 200, Date = new DateOnly(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 },
            new Transaction { Id = 4, Amount = 999, Date = new DateOnly(2026, 1, 4), UserId = 2, SourceId = 2, CurrencyId = 1, TransactionTypeId = 1 }
        );
        await dbContext.SaveChangesAsync();

        var service = new TransactionService(dbContext, GetMockUserContext(1), new NotificationContext(), Mock.Of<IBalanceManagementService>());

        var input = new GetUserTransactionsInput
        {
            FromDate = new DateOnly(2026, 1, 2),
            ToDate = new DateOnly(2026, 1, 3),
            SortBy = TransactionSortBy.Amount,
            SortDescending = true,
            Offset = 0,
            Limit = 10
        };

        var result = await service.GetUsersTransactions(input);

        result.Data.Should().HaveCount(2);
        result.Data.First().Amount.Should().Be(300);
        result.Data.Last().Amount.Should().Be(100);
    }

    [Fact]
    public async Task GetUsersTransactions_FiltersByCategory()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Food", UserId = 1 },
            new Category { Id = 2, Name = "Transport", UserId = 1 }
        );

        dbContext.Transactions.AddRange(
            new Transaction { Id = 40, Amount = 100, Date = new DateOnly(2026, 1, 2), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1, CategoryId = 1 },
            new Transaction { Id = 41, Amount = 200, Date = new DateOnly(2026, 1, 3), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1, CategoryId = 2 }
        );
        await dbContext.SaveChangesAsync();

        var service = new TransactionService(dbContext, GetMockUserContext(1), new NotificationContext(), Mock.Of<IBalanceManagementService>());

        var result = await service.GetUsersTransactions(new GetUserTransactionsInput { CategoryId = 1, Limit = 10, Offset = 0 });

        result.Data.Should().ContainSingle();
        result.Data.First().Category!.Id.Should().Be(1);
    }

    private static async Task SeedTransactionDependencies(API.AppDbContext dbContext, int userId)
    {
        if (!dbContext.Currencies.Any(c => c.Id == 1))
            dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });

        if (!dbContext.TransactionTypes.Any(t => t.Id == 1))
            dbContext.TransactionTypes.Add(new TransactionType { Id = 1, Name = "Expense" });

        if (!dbContext.TransactionTypes.Any(t => t.Id == (int)TransactionTypeEnum.Transfer))
            dbContext.TransactionTypes.Add(new TransactionType { Id = (int)TransactionTypeEnum.Transfer, Name = "Transfer" });

        if (!dbContext.TransactionTypes.Any(t => t.Id == (int)TransactionTypeEnum.Adjustment))
            dbContext.TransactionTypes.Add(new TransactionType { Id = (int)TransactionTypeEnum.Adjustment, Name = "Adjustment" });

        if (!dbContext.Sources.Any(s => s.Id == userId))
            dbContext.Sources.Add(new Source { Id = userId, Name = $"Source{userId}", Amount = 1000, UserId = userId, CurrencyId = 1, IsArchived = false });

        await dbContext.SaveChangesAsync();
    }
}
