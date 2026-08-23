using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Services;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Transactions;

public class BalanceManagerTest
{
    [Fact]
    public void Apply_Expense_DecreasesSourceAmount()
    {
        var manager = new BalanceManager();
        var source = new Source { Id = 1, Name = "Wallet", Amount = 500, UserId = 1, CurrencyId = 1 };
        var transaction = new Transaction { Amount = 120, SourceId = 1, TransactionTypeId = (int)TransactionTypeEnum.Expense };

        var result = manager.Apply(transaction, source, null);

        result.IsSuccess.Should().BeTrue();
        source.Amount.Should().Be(380);
    }

    [Fact]
    public void Apply_Transfer_MovesMoneyBetweenSources()
    {
        var manager = new BalanceManager();
        var source = new Source { Id = 1, Name = "Main", Amount = 1000, UserId = 1, CurrencyId = 1 };
        var destination = new Source { Id = 2, Name = "Savings", Amount = 300, UserId = 1, CurrencyId = 1 };
        var transaction = new Transaction { Amount = 200, SourceId = 1, DestinationSourceId = 2, TransactionTypeId = (int)TransactionTypeEnum.Transfer };

        var result = manager.Apply(transaction, source, destination);

        result.IsSuccess.Should().BeTrue();
        source.Amount.Should().Be(800);
        destination.Amount.Should().Be(500);
    }

    [Fact]
    public void Apply_Transfer_Fails_WhenDestinationMissing()
    {
        var manager = new BalanceManager();
        var source = new Source { Id = 1, Name = "Main", Amount = 1000, UserId = 1, CurrencyId = 1 };
        var transaction = new Transaction { Amount = 200, SourceId = 1, DestinationSourceId = 99, TransactionTypeId = (int)TransactionTypeEnum.Transfer };

        var result = manager.Apply(transaction, source, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SourceErrors.DestinationNotFound(99).Code);
        source.Amount.Should().Be(1000);
    }

    [Fact]
    public void Apply_UnknownTransactionType_ReturnsFailure()
    {
        var manager = new BalanceManager();
        var source = new Source { Id = 1, Name = "Main", Amount = 1000, UserId = 1, CurrencyId = 1 };
        var transaction = new Transaction { Amount = 200, SourceId = 1, TransactionTypeId = 999 };

        var result = manager.Apply(transaction, source, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionErrors.UnknownTransactionType(999).Code);
        source.Amount.Should().Be(1000);
    }

    [Fact]
    public void Revert_Transfer_RevertsAmounts()
    {
        var manager = new BalanceManager();
        var source = new Source { Id = 1, Name = "Main", Amount = 800, UserId = 1, CurrencyId = 1 };
        var destination = new Source { Id = 2, Name = "Savings", Amount = 500, UserId = 1, CurrencyId = 1 };
        var transaction = new Transaction { Amount = 200, SourceId = 1, DestinationSourceId = 2, TransactionTypeId = (int)TransactionTypeEnum.Transfer };

        var result = manager.Revert(transaction, source, destination);

        result.IsSuccess.Should().BeTrue();
        source.Amount.Should().Be(1000);
        destination.Amount.Should().Be(300);
    }

    [Fact]
    public void Revert_Adjustment_ReturnsAdjustmentNotReversible()
    {
        var manager = new BalanceManager();
        var source = new Source { Id = 1, Name = "Main", Amount = 800, UserId = 1, CurrencyId = 1 };
        var transaction = new Transaction { Amount = 200, SourceId = 1, TransactionTypeId = (int)TransactionTypeEnum.Adjustment };

        var result = manager.Revert(transaction, source, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionErrors.AdjustmentNotReversible.Code);
        source.Amount.Should().Be(800);
    }

    [Fact]
    public void IsBalanceAffected_DetectsChangedAmount()
    {
        var manager = new BalanceManager();
        var original = new Transaction { Amount = 100, TransactionTypeId = 1, SourceId = 1, DestinationSourceId = null };
        var updated = new Transaction { Amount = 150, TransactionTypeId = 1, SourceId = 1, DestinationSourceId = null };

        manager.IsBalanceAffected(original, updated).Should().BeTrue();
    }

    [Fact]
    public void IsBalanceAffected_ReturnsFalse_WhenNothingRelevantChanged()
    {
        var manager = new BalanceManager();
        var original = new Transaction { Amount = 100, TransactionTypeId = 1, SourceId = 1, DestinationSourceId = null };
        var updated = new Transaction { Amount = 100, TransactionTypeId = 1, SourceId = 1, DestinationSourceId = null };

        manager.IsBalanceAffected(original, updated).Should().BeFalse();
    }
}
