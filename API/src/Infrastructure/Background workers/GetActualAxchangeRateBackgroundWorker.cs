using FinancialPlanner.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinancialPlanner.Infrastructure.Background_workers;

public sealed class GetActualExchangeRateBackgroundWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<GetActualExchangeRateBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Exchange rate worker started: daily at 00:00 UTC.");

        await RunAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            try
            {
                await Task.Delay(now.Date.AddDays(1) - now, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            await RunAsync(stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Exchange rate sync started at {Time}.", DateTimeOffset.Now);

            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();

            await service.RefreshAsync(ct);

            logger.LogInformation("Exchange rate sync finished.");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exchange rate sync failed; will retry at next run.");
        }
    }
}