using API.Inputs;
using API.Models;
using API.Utils.Notification;
using API.Utils.UserContext;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Frequency;

public class FrequencyService(AppDbContext context, NotificationContext notificationContext, ICurrentUserProvider currentUserProvider) : IFrequencyService
{
   public async Task<FrequencyDto?> CreateFrequency(FrequencyInput frequency)
   {
       var userId = currentUserProvider.RequiredUserId;
       var intervalUnit = await context.IntervalUnits.FindAsync(frequency.IntervalUnitId);

       if (intervalUnit == null)
       {
           notificationContext.AddNotification("Interval unit not found", ErrorType.NotFound);
           return null;
       }

       var newFrequency = new Models.Frequency
       {
           Name = frequency.Name,
           UserId = userId,
           IntervalUnitId = frequency.IntervalUnitId,
           IntervalValue = frequency.IntervalValue
       };

       var result = context.Frequencies.Add(newFrequency);

       await context.SaveChangesAsync();

       return CreateFrequencyDto(result.Entity);
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
            .Select(f => CreateFrequencyDto(f))
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

        return CreateFrequencyDto(frequency);
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
            .. frequencies.Select(CreateFrequencyDto)
        ];
    }

        public async Task<FrequencyDto?> UpdateFrequency(FrequencyInput frequency, int id)
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

            existingFrequency.Name = frequency.Name;
            existingFrequency.IntervalValue = frequency.IntervalValue;
            existingFrequency.IntervalUnitId = frequency.IntervalUnitId;

            await context.SaveChangesAsync();

            return CreateFrequencyDto(existingFrequency);
        }

    private static FrequencyDto CreateFrequencyDto(Models.Frequency frequency)
    {
        return new FrequencyDto
        {
            Id = frequency.Id,
            Name = frequency.Name,
            UserId = frequency.UserId,
            IntervalValue = frequency.IntervalValue,
            IntervalUnit = new IntervalUnitDto
            {
                Id = frequency.IntervalUnitId,
                Name = frequency.IntervalUnitNavigation.Name
            }
        };
    }
}