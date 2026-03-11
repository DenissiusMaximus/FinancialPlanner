using API.Models;
using API.Utils.Notification;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Frequency;

public class FrequencyService(AppDbContext context, NotificationContext notificationContext) : IFrequencyService
{
   public async Task<FrequencyDto?> CreateFrequency(FrequencyInput frequency, int userId)
   {
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

    public async Task<bool> DeleteFrequency(int id, int userId)
    {
        var frequency = context.Frequencies.FirstOrDefault(f => f.Id == id && f.UserId == userId);

        if (frequency == null)
        {
            notificationContext.AddNotification("Frequency not found", ErrorType.NotFound);
            return false;
        }

        context.Frequencies.Remove(frequency);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<List<FrequencyDto>> GetFrequencies(int userId)
    {
        var frequencies = await context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .Where(f => f.UserId == userId || f.UserId == null)
            .Select(f => CreateFrequencyDto(f))
            .ToListAsync();

        return frequencies;
    }

    public async Task<FrequencyDto?> GetFrequency(int id, int userId)
    {
        var frequency = context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .FirstOrDefault(f => f.Id == id && f.UserId == userId);

        if (frequency == null)
        {
            notificationContext.AddNotification("Frequency not found", ErrorType.NotFound);
            return null;
        }

        return CreateFrequencyDto(frequency);
    }

    public async Task<List<FrequencyDto>> GetUserFrequencies(int userId)
    {
        var frequencies = context.Frequencies
            .AsNoTracking()
            .Include(f => f.IntervalUnitNavigation)
            .Where(f => f.UserId == userId)
            .ToList();

        return
        [
            .. frequencies.Select(CreateFrequencyDto)
        ];
    }

        public async Task<FrequencyDto?> UpdateFrequency(FrequencyInput frequency, int id, int userId)
        {
            var existingFrequency = context.Frequencies
                .Include(f => f.IntervalUnitNavigation)
                .FirstOrDefault(f => f.Id == id && f.UserId == userId);

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