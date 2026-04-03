using API.Domain.Calculator;
using API.Dtos;
using API.Inputs;
using API.Models;
using FluentAssertions;

namespace APITest;

public class AimProgressCalculatorTest
{
    [Fact]
    public async Task CalculateAimProgress_SetsCollectedAndTargetAmounts_ForSingleAim()
    {
        var calculator = new AimProgressCalculator();

        var usd = new CurrencyDto { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source = new SourceDtoLookup { Id = 10, Name = "Wallet", Amount = 300m, Currency = usd, UserId = 1 };

        var aims = new List<AimDto>
        {
            new()
            {
                Id = 1,
                Name = "Laptop",
                Amount = 1000m,
                Priority = 1,
                UserId = 1,
                Currency = usd,
                Sources = [source]
            }
        };

        var result = await calculator.CalculateAimProgress(aims);
        var aim = result.Single();

        aim.Progress.Should().NotBeNull();
        aim.Progress!.CollectedAmount.Should().Be(300m);
        aim.Progress.TargetAmount.Should().Be(1000m);
        aim.Progress.CompletionPercentage.Should().Be(30m);
    }

    [Fact]
    public async Task CalculateAimProgress_UsesPriority_WhenAimsShareSameSource()
    {
        var calculator = new AimProgressCalculator();

        var usd = new CurrencyDto { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var sharedSource = new SourceDtoLookup { Id = 15, Name = "Savings", Amount = 100m, Currency = usd, UserId = 1 };

        var aims = new List<AimDto>
        {
            new()
            {
                Id = 1,
                Name = "Low priority",
                Amount = 100m,
                Priority = 2,
                UserId = 1,
                Currency = usd,
                Sources = [sharedSource]
            },
            new()
            {
                Id = 2,
                Name = "High priority",
                Amount = 100m,
                Priority = 1,
                UserId = 1,
                Currency = usd,
                Sources = [sharedSource]
            }
        };

        var result = await calculator.CalculateAimProgress(aims);
        var highPriorityAim = result.Single(a => a.Id == 2);
        var lowPriorityAim = result.Single(a => a.Id == 1);

        highPriorityAim.Progress!.CollectedAmount.Should().Be(100m);
        lowPriorityAim.Progress!.CollectedAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAimProgress_ConvertsCurrency_WhenSourceAndAimCurrenciesDiffer()
    {
        var calculator = new AimProgressCalculator();

        var usd = new CurrencyDto { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var uah = new CurrencyDto { Id = 2, Name = "UAH", UsdExchangeRate = 0.025m };

        var sourceInUsd = new SourceDtoLookup { Id = 30, Name = "USD source", Amount = 100m, Currency = usd, UserId = 1 };

        var aims = new List<AimDto>
        {
            new()
            {
                Id = 3,
                Name = "UAH aim",
                Amount = 5000m,
                Priority = 1,
                UserId = 1,
                Currency = uah,
                Sources = [sourceInUsd]
            }
        };

        var result = await calculator.CalculateAimProgress(aims);
        var aim = result.Single();

        aim.Progress!.CollectedAmount.Should().Be(4000m);
        aim.Progress.TargetAmount.Should().Be(5000m);
        aim.Progress.CompletionPercentage.Should().Be(80m);
    }

    [Fact]
    public async Task CalculateAimProgress_DistributesFromMultipleSources_ForSingleAim()
    {
        var calculator = new AimProgressCalculator();

        var usd = new CurrencyDto { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source1 = new SourceDtoLookup { Id = 51, Name = "Card", Amount = 200m, Currency = usd, UserId = 1 };
        var source2 = new SourceDtoLookup { Id = 52, Name = "Cash", Amount = 150m, Currency = usd, UserId = 1 };

        var aims = new List<AimDto>
        {
            new()
            {
                Id = 5,
                Name = "Phone",
                Amount = 300m,
                Priority = 1,
                UserId = 1,
                Currency = usd,
                Sources = [source1, source2]
            }
        };

        var result = await calculator.CalculateAimProgress(aims);
        var aim = result.Single();

        aim.Progress!.CollectedAmount.Should().Be(300m);
        source1.Amount.Should().Be(0m);
        source2.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task CalculateAimProgress_CarriesExcessToNextAim_ByPriority()
    {
        var calculator = new AimProgressCalculator();

        var usd = new CurrencyDto { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var sharedSource = new SourceDtoLookup { Id = 61, Name = "Savings", Amount = 250m, Currency = usd, UserId = 1 };

        var aims = new List<AimDto>
        {
            new()
            {
                Id = 6,
                Name = "High priority",
                Amount = 100m,
                Priority = 1,
                UserId = 1,
                Currency = usd,
                Sources = [sharedSource]
            },
            new()
            {
                Id = 7,
                Name = "Second priority",
                Amount = 200m,
                Priority = 2,
                UserId = 1,
                Currency = usd,
                Sources = [sharedSource]
            }
        };

        var result = await calculator.CalculateAimProgress(aims);
        var firstAim = result.Single(a => a.Id == 6);
        var secondAim = result.Single(a => a.Id == 7);

        firstAim.Progress!.CollectedAmount.Should().Be(100m);
        secondAim.Progress!.CollectedAmount.Should().Be(150m);
        sharedSource.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAimProgress_LeavesExpectedRemainder_OnSourceAfterPartialFunding()
    {
        var calculator = new AimProgressCalculator();

        var usd = new CurrencyDto { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source = new SourceDtoLookup { Id = 71, Name = "Deposit", Amount = 500m, Currency = usd, UserId = 1 };

        var aims = new List<AimDto>
        {
            new()
            {
                Id = 8,
                Name = "Short goal",
                Amount = 120m,
                Priority = 1,
                UserId = 1,
                Currency = usd,
                Sources = [source]
            }
        };

        var result = await calculator.CalculateAimProgress(aims);
        var aim = result.Single();

        aim.Progress!.CollectedAmount.Should().Be(120m);
        source.Amount.Should().Be(380m);
    }

    [Fact]
    public async Task CalculateAimProgress_Throws_WhenAimCurrencyIsNull()
    {
        var calculator = new AimProgressCalculator();

        var usd = new CurrencyDto { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var source = new SourceDtoLookup { Id = 40, Name = "Wallet", Amount = 100m, Currency = usd, UserId = 1 };

        var aims = new List<AimDto>
        {
            new()
            {
                Id = 4,
                Name = "Broken aim",
                Amount = 1000m,
                Priority = 1,
                UserId = 1,
                Currency = null,
                Sources = [source]
            }
        };

        var act = async () => await calculator.CalculateAimProgress(aims);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*has null currency*");
    }
}
