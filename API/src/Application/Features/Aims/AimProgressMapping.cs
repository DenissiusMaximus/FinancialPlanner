using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Features.Aims;

internal static class AimProgressMapping
{
    public static void ApplyProgress(AimDto dto, Aim aim, AimProgressCalculation calculation)
    {
        if (calculation.ProgressByAimId.TryGetValue(aim.Id, out var progress))
        {
            dto.Progress = new AimProgressDto
            {
                CollectedAmount = progress.CollectedAmount,
                TargetAmount = progress.TargetAmount
            };
        }

        if (dto.Sources is null)
            return;

        foreach (var source in dto.Sources)
        {
            if (calculation.RemainingAmountBySourceId.TryGetValue(source.Id, out var remaining))
                source.Amount = remaining;
        }
    }
}
