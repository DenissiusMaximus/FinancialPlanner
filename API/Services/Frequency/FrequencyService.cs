using API.Inputs;
using API.Models;
using API.Utils.Map;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Frequency;

public class FrequencyService(AppDbContext context, NotificationContext notificationContext, ICurrentUserContext currentUserProvider) : IFrequencyService
{
   public async Task<FrequencyDto?> CreateFrequency(CreateFrequencyInput frequency)
   {
       var userId = currentUserProvider.RequiredUserId;
       var intervalUnit = await context.IntervalUnits.FindAsync(frequency.IntervalUnitId);

       if (intervalUnit == null)
       {
           notificationContext.AddNotification("Interval unit not found", ErrorType.NotFound);
           return null;
       }

        var newFrequency = frequency.Adapt<Models.Frequency>();
       
        newFrequency.UserId = userId;

       var result = context.Frequencies.Add(newFrequency);

       await context.SaveChangesAsync();

       return result.Entity.Adapt<FrequencyDto>();
   }

    public async Task<bool> DeleteFrequency(int id)
    {
        var userId = currentUserProvider.RequiredUserId;
        var frequency = await context.Frequencies.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (frequency == null)
        {
            notificationContext.AddNotification("Frequency not found", ErrorType.NotFound);
            return false;
        }

        context.Frequencies.Remove(frequency);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<IReadOnlyCollection<FrequencyDto>> GetFrequencies()
    {
        var userId = currentUserProvider.RequiredUserId;
        var frequencies = await context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .Where(f => f.UserId == userId || f.UserId == null)
            .Select(f => f.Adapt<FrequencyDto>())
            .ToListAsync();

        return frequencies;
    }

    public async Task<FrequencyDto?> GetFrequency(int id)
    {
        var userId = currentUserProvider.RequiredUserId;
        var frequency = await context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (frequency == null)
        {
            notificationContext.AddNotification("Frequency not found", ErrorType.NotFound);
            return null;
        }

        return frequency.Adapt<FrequencyDto>();
    }

    public async Task<IReadOnlyCollection<FrequencyDto>> GetUserFrequencies()
    {
        var userId = currentUserProvider.RequiredUserId;
        var frequencies = await context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .Where(f => f.UserId == userId)
            .ToListAsync();

        return
        [
            .. frequencies.Select(f => f.Adapt<FrequencyDto>())
        ];
    }

        public async Task<FrequencyDto?> UpdateFrequency(UpdateFrequencyInput frequency, int id)
        {
            var userId = currentUserProvider.RequiredUserId;
            var existingFrequency = await context.Frequencies
                .Include(f => f.IntervalUnitNavigation)
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (existingFrequency == null)
            {
                notificationContext.AddNotification("Frequency not found", ErrorType.NotFound);
                return null;
            }

            frequency.AdaptIgnoreNull(existingFrequency);

            await context.SaveChangesAsync();

            return existingFrequency.Adapt<FrequencyDto>();
        }
}