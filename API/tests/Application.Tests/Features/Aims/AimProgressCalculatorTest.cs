using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Services;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Aims;

public class AimProgressCalculatorTest
{
    private static SourceAim Link(Aim aim, Source source)
    {
        var sourceAim = new SourceAim { AimId = aim.Id, SourceId = source.Id, Aim = aim, Source = source };
        aim.SourceAims.Add(sourceAim);
        return sourceAim;
    }

    [Fact]
    public void Calculate_SetsCollectedAndTargetAmounts_ForSingleAim()
    {
        var calculator = new AimProgressCalculator();

        var usd = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source = new Source { Id = 10, Name = "Wallet", Amount = 300m, Currency = usd, CurrencyId = 1, UserId = 1 };
        var aim = new Aim { Id = 1, Name = "Laptop", Amount = 1000m, Priority = 1, UserId = 1, Currency = usd, CurrencyId = 1 };
        Link(aim, source);

        var result = calculator.Calculate([aim]);

        result.IsSuccess.Should().BeTrue();
        var progress = result.Value.ProgressByAimId[aim.Id];
        progress.CollectedAmount.Should().Be(300m);
        progress.TargetAmount.Should().Be(1000m);
        progress.CompletionPercentage.Should().Be(30m);
    }

    [Fact]
    public void Calculate_UsesPriority_WhenAimsShareSameSource()
    {
        var calculator = new AimProgressCalculator();

        var usd = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var sharedSource = new Source { Id = 15, Name = "Savings", Amount = 100m, Currency = usd, CurrencyId = 1, UserId = 1 };

        var lowPriorityAim = new Aim { Id = 1, Name = "Low priority", Amount = 100m, Priority = 2, UserId = 1, Currency = usd, CurrencyId = 1 };
        var highPriorityAim = new Aim { Id = 2, Name = "High priority", Amount = 100m, Priority = 1, UserId = 1, Currency = usd, CurrencyId = 1 };
        Link(lowPriorityAim, sharedSource);
        Link(highPriorityAim, sharedSource);

        var result = calculator.Calculate([lowPriorityAim, highPriorityAim]);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProgressByAimId[2].CollectedAmount.Should().Be(100m);
        result.Value.ProgressByAimId[1].CollectedAmount.Should().Be(0m);
    }

    [Fact]
    public void Calculate_ConvertsCurrency_WhenSourceAndAimCurrenciesDiffer()
    {
        var calculator = new AimProgressCalculator();

        var usd = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var uah = new Currency { Id = 2, Name = "UAH", UsdExchangeRate = 0.025m };

        var sourceInUsd = new Source { Id = 30, Name = "USD source", Amount = 100m, Currency = usd, CurrencyId = 1, UserId = 1 };
        var aim = new Aim { Id = 3, Name = "UAH aim", Amount = 5000m, Priority = 1, UserId = 1, Currency = uah, CurrencyId = 2 };
        Link(aim, sourceInUsd);

        var result = calculator.Calculate([aim]);

        result.IsSuccess.Should().BeTrue();
        var progress = result.Value.ProgressByAimId[aim.Id];
        progress.CollectedAmount.Should().Be(4000m);
        progress.TargetAmount.Should().Be(5000m);
        progress.CompletionPercentage.Should().Be(80m);
    }

    [Fact]
    public void Calculate_DistributesFromMultipleSources_ForSingleAim()
    {
        var calculator = new AimProgressCalculator();

        var usd = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source1 = new Source { Id = 51, Name = "Card", Amount = 200m, Currency = usd, CurrencyId = 1, UserId = 1 };
        var source2 = new Source { Id = 52, Name = "Cash", Amount = 150m, Currency = usd, CurrencyId = 1, UserId = 1 };

        var aim = new Aim { Id = 5, Name = "Phone", Amount = 300m, Priority = 1, UserId = 1, Currency = usd, CurrencyId = 1 };
        Link(aim, source1);
        Link(aim, source2);

        var result = calculator.Calculate([aim]);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProgressByAimId[aim.Id].CollectedAmount.Should().Be(300m);
        result.Value.RemainingAmountBySourceId[source1.Id].Should().Be(0m);
        result.Value.RemainingAmountBySourceId[source2.Id].Should().Be(50m);
    }

    [Fact]
    public void Calculate_CarriesExcessToNextAim_ByPriority()
    {
        var calculator = new AimProgressCalculator();

        var usd = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var sharedSource = new Source { Id = 61, Name = "Savings", Amount = 250m, Currency = usd, CurrencyId = 1, UserId = 1 };

        var firstAim = new Aim { Id = 6, Name = "High priority", Amount = 100m, Priority = 1, UserId = 1, Currency = usd, CurrencyId = 1 };
        var secondAim = new Aim { Id = 7, Name = "Second priority", Amount = 200m, Priority = 2, UserId = 1, Currency = usd, CurrencyId = 1 };
        Link(firstAim, sharedSource);
        Link(secondAim, sharedSource);

        var result = calculator.Calculate([firstAim, secondAim]);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProgressByAimId[6].CollectedAmount.Should().Be(100m);
        result.Value.ProgressByAimId[7].CollectedAmount.Should().Be(150m);
        result.Value.RemainingAmountBySourceId[sharedSource.Id].Should().Be(0m);
    }

    [Fact]
    public void Calculate_LeavesExpectedRemainder_OnSourceAfterPartialFunding()
    {
        var calculator = new AimProgressCalculator();

        var usd = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source = new Source { Id = 71, Name = "Deposit", Amount = 500m, Currency = usd, CurrencyId = 1, UserId = 1 };
        var aim = new Aim { Id = 8, Name = "Short goal", Amount = 120m, Priority = 1, UserId = 1, Currency = usd, CurrencyId = 1 };
        Link(aim, source);

        var result = calculator.Calculate([aim]);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProgressByAimId[aim.Id].CollectedAmount.Should().Be(120m);
        result.Value.RemainingAmountBySourceId[source.Id].Should().Be(380m);
    }

    [Fact]
    public void Calculate_ReturnsFailure_WhenAimCurrencyIsNull()
    {
        var calculator = new AimProgressCalculator();

        var usd = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source = new Source { Id = 40, Name = "Wallet", Amount = 100m, Currency = usd, CurrencyId = 1, UserId = 1 };
        var aim = new Aim { Id = 4, Name = "Broken aim", Amount = 1000m, Priority = 1, UserId = 1, Currency = null };
        Link(aim, source);

        var result = calculator.Calculate([aim]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.CurrencyMissing(aim.Id).Code);
    }
}
