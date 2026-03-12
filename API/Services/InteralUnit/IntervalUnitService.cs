using API.Models;
using API.Utils.Notification;
using Microsoft.EntityFrameworkCore;

namespace API.Services.InteralUnit;

public class IntervalUnitService(AppDbContext context, NotificationContext notificationContext) : IIntervalUnitService
{
    public async Task<List<IntervalUnitDto>> GetIntervalUnits()
    {
        var intervalUnits = await context.IntervalUnits
            .AsNoTracking()
            .Select(iu => new IntervalUnitDto
            {
                Id = iu.Id,
                Name = iu.Name
            })
            .ToListAsync();

        return intervalUnits;
    }

    public async Task<IntervalUnitDto?> GetIntervalUnitById(int id)
    {
        var intervalUnit = await context.IntervalUnits
            .AsNoTracking()
            .Where(iu => iu.Id == id)
            .Select(iu => new IntervalUnitDto
            {
                Id = iu.Id,
                Name = iu.Name
            })
            .FirstOrDefaultAsync();

        if (intervalUnit == null)
        {
            notificationContext.AddNotification("Interval unit not found.", ErrorType.NotFound);
            return null;
        }

        return intervalUnit;
    }
}