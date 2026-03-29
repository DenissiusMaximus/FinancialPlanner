using System;
using API.Dtos;
using API.Inputs;
using API.Models;
using Mapster;
using System.Linq;

namespace API.Domain.Calculator;

public interface IAimProgressCalculator
{
    Task<IReadOnlyCollection<AimDto>> CalculateAimProgress(List<AimDto> aims);
}

public class AimProgressCalculator : IAimProgressCalculator
{
    public async Task<IReadOnlyCollection<AimDto>> CalculateAimProgress(List<AimDto> aims)
    {
        var sources = DetermineInvolvedSources(aims);

        aims.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var aim in aims)
        {
            var aimGoal = aim.Amount;

            if (aim.Sources == null)
                continue;

            foreach (var source in sources.Where(s => aim.Sources.Any(a => a.Id == s.Id)))
            {
                if (source.Amount <= 0) continue;

                if (aim.Currency! == null)
                    throw new Exception($"Aim with id {aim.Id} has null currency.");

                var sourceAmountInAimCurrency = AmountInCurrency(source.Amount, source.Currency, aim.Currency);

                var goalDeficit = aimGoal - sourceAmountInAimCurrency;

                if (goalDeficit <= 0)
                {
                    var excessInAimCurrency = Math.Abs(goalDeficit);
                    source.Amount = AmountInCurrency(excessInAimCurrency, aim.Currency, source.Currency);
                    aimGoal = 0;
                    break;
                }
                source.Amount = 0;
                aimGoal = goalDeficit;
            }

            aim.Progress ??= new();
            aim.Progress.CollectedAmount = aim.Amount - aimGoal;
            aim.Progress.TargetAmount = aim.Amount;
        }

        return aims;
    }

    private static decimal AmountInCurrency(decimal amount, CurrencyDto fromCurrency, CurrencyDto toCurrency)
    {
        if (fromCurrency.Id == toCurrency.Id)
            return amount;

        return amount * fromCurrency.UsdExchangeRate / toCurrency.UsdExchangeRate;
    }

    private static List<SourceDtoLookup> DetermineInvolvedSources(List<AimDto> aims)
    {
        return [.. aims
        .Where(a => a.Sources != null)   
        .SelectMany(a => a.Sources!)     
        .DistinctBy(s => s.Id)];
    }
}
