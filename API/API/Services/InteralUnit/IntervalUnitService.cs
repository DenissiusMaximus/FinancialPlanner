using API.Models;
using API.Utils.Notification;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.InteralUnit;

public class IntervalUnitService(AppDbContext context, NotificationContext notificationContext) : IIntervalUnitService
{
    public async Task<IReadOnlyCollection<IntervalUnitDto>> GetIntervalUnits()
    {
        var intervalUnits = await context.IntervalUnits
            .AsNoTracking()
            .Select(iu => iu.Adapt<IntervalUnitDto>())
            .ToListAsync();

        return intervalUnits;
    }

    public async Task<IntervalUnitDto?> GetIntervalUnitById(int id)
    {
        var intervalUnit = await context.IntervalUnits
            .AsNoTracking()
            .Where(iu => iu.Id == id)
            .Select(iu => iu.Adapt<IntervalUnitDto>())
            .FirstOrDefaultAsync();

        if (intervalUnit == null)
        {
            notificationContext.AddNotification("Interval unit not found.", ErrorType.NotFound);
            return null;
        }

        return intervalUnit;
    }
}