namespace FinancialPlanner.Application.Abstractions;

public interface IExchangeRateService
{
    Task RefreshAsync(CancellationToken ct);
}
