namespace FinancialPlanner.Application.Features.Aims.Dtos;

public class AimProgressDto
{
    public decimal CollectedAmount { get; set; }

    public decimal TargetAmount { get; set; }

    public decimal CompletionPercentage => TargetAmount > 0 ? Math.Round(CollectedAmount / TargetAmount * 100, 2) : 0;
}
