using API.Domain.BalanceManagement;
using API.Models;
using API.Services.Transaction;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class BalanceManagementServiceTest : BaseTest
{
    [Fact]
    public async Task UpdateAmounts_Expense_DecreasesSourceAmount()
    {
        var dbContext = GetInMemoryDbContext();
        var source = new Source { Id = 1, Name = "Wallet", Amount = 500, UserId = 1, CurrencyId = 1, IsArchived = false };
        dbContext.Sources.Add(source);
        await dbContext.SaveChangesAsync();

        var service = new BalanceManagementService(dbContext, new NotificationContext());

        var result = await service.UpdateAmounts(
            new Transaction { Amount = 120, SourceId = 1, TransactionTypeId = (int)TransactionTypeEnum.Expense },
            source,
            1);

        result.Should().BeTrue();
        source.Amount.Should().Be(380);
    }

    [Fact]
    public async Task UpdateAmounts_Transfer_MovesMoneyBetweenSources()
    {
        var dbContext = GetInMemoryDbContext();
        var source = new Source { Id = 1, Name = "Main", Amount = 1000, UserId = 1, CurrencyId = 1, IsArchived = false };
        var destination = new Source { Id = 2, Name = "Savings", Amount = 300, UserId = 1, CurrencyId = 1, IsArchived = false };
        dbContext.Sources.AddRange(source, destination);
        await dbContext.SaveChangesAsync();

        var service = new BalanceManagementService(dbContext, new NotificationContext());

        var result = await service.UpdateAmounts(
            new Transaction
            {
                Amount = 200,
                SourceId = 1,
                DestinationSourceId = 2,
                TransactionTypeId = (int)TransactionTypeEnum.Transfer
            },
            source,
            1);

        result.Should().BeTrue();
        source.Amount.Should().Be(800);
        destination.Amount.Should().Be(500);
    }

    [Fact]
    public async Task UpdateAmounts_Transfer_ReturnsFalse_WhenDestinationMissing()
    {
        var dbContext = GetInMemoryDbContext();
        var source = new Source { Id = 1, Name = "Main", Amount = 1000, UserId = 1, CurrencyId = 1, IsArchived = false };
        dbContext.Sources.Add(source);
        await dbContext.SaveChangesAsync();

        var notificationContext = new NotificationContext();
        var service = new BalanceManagementService(dbContext, notificationContext);

        var result = await service.UpdateAmounts(
            new Transaction
            {
                Amount = 200,
                SourceId = 1,
                DestinationSourceId = 99,
                TransactionTypeId = (int)TransactionTypeEnum.Transfer
            },
            source,
            1);

        result.Should().BeFalse();
        notificationContext.Notifications.Should().ContainSingle().Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ResetTransaction_Transfer_RevertsAmounts()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Sources.AddRange(
            new Source { Id = 1, Name = "Main", Amount = 800, UserId = 1, CurrencyId = 1, IsArchived = false },
            new Source { Id = 2, Name = "Savings", Amount = 500, UserId = 1, CurrencyId = 1, IsArchived = false }
        );
        await dbContext.SaveChangesAsync();

        var service = new BalanceManagementService(dbContext, new NotificationContext());

        var result = await service.ResetTransaction(new Transaction
        {
            Amount = 200,
            SourceId = 1,
            DestinationSourceId = 2,
            TransactionTypeId = (int)TransactionTypeEnum.Transfer
        }, 1);

        var source = await dbContext.Sources.FindAsync(1);
        var destination = await dbContext.Sources.FindAsync(2);

        result.Should().BeTrue();
        source!.Amount.Should().Be(1000);
        destination!.Amount.Should().Be(300);
    }
}
