using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;

namespace FinancialPlanner.Domain.Services;

public sealed class AimProgressCalculator : IAimProgressCalculator
{
    public Result<AimProgressCalculation> Calculate(IReadOnlyList<Aim> aims)
    {
        var sources = DetermineInvolvedSources(aims);
        var remaining = sources.ToDictionary(s => s.Id, s => s.Amount);

        var sortedAims = aims.OrderBy(a => a.Priority).ToList();
        var progress = new Dictionary<int, AimProgress>();

        foreach (var aim in sortedAims)
        {
            var aimGoal = aim.Amount;
            var aimSourceIds = aim.SourceAims.Select(sa => sa.SourceId).ToHashSet();

            foreach (var source in sources.Where(s => aimSourceIds.Contains(s.Id)))
            {
                if (!remaining.TryGetValue(source.Id, out var sourceAmount) || sourceAmount <= 0)
                    continue;

                if (aim.Currency is null)
                    return Result.Failure<AimProgressCalculation>(AimErrors.CurrencyMissing(aim.Id));

                var sourceAmountInAimCurrency = AmountInCurrency(sourceAmount, source.Currency, aim.Currency);
                var goalDeficit = aimGoal - sourceAmountInAimCurrency;

                if (goalDeficit <= 0)
                {
                    var excessInAimCurrency = Math.Abs(goalDeficit);
                    remaining[source.Id] = AmountInCurrency(excessInAimCurrency, aim.Currency, source.Currency);
                    aimGoal = 0;
                    break;
                }

                remaining[source.Id] = 0;
                aimGoal = goalDeficit;
            }

            progress[aim.Id] = new AimProgress
            {
                CollectedAmount = aim.Amount - aimGoal,
                TargetAmount = aim.Amount
            };
        }

        return new AimProgressCalculation
        {
            ProgressByAimId = progress,
            RemainingAmountBySourceId = remaining
        };
    }

    private static decimal AmountInCurrency(decimal amount, Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency.Id == toCurrency.Id)
            return amount;

        return amount * fromCurrency.UsdExchangeRate / toCurrency.UsdExchangeRate;
    }

    private static List<Source> DetermineInvolvedSources(IReadOnlyList<Aim> aims)
    {
        return aims
            .SelectMany(a => a.SourceAims.Select(sa => sa.Source))
            .DistinctBy(s => s.Id)
            .ToList();
    }
}
