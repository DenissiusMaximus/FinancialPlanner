using System;
using API.Dtos;
using API.Utils.Notification;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Source;

public class SourceService(AppDbContext context, NotificationContext notificationContext) : ISourceService
{
    public async Task<SourceDto?> GetSourceById(int sourceId, int userId)
    {
        var source = await context.Sources
            .AsNoTracking()
            .Include(s => s.Currency)
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .Select(s => CreateSourceDto(s))
            .FirstOrDefaultAsync();

        return source;
    }

    public async Task<IReadOnlyCollection<SourceDto>> GetSources(int userId)
    {
        var result = await context.Sources
                .AsNoTracking()
            .Include(s => s.Currency)
            .Where(s => s.UserId == userId && !s.IsArchived)
            .Select(s => CreateSourceDto(s))
            .ToListAsync();

        return result;
    }

    public async Task<SourceDto?> CreateSource(CreateSourceInput createSourceDto, int userId)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == createSourceDto.CurrencyId);

        if (currency == null)
        {
            notificationContext.AddNotification("Currency not found", ErrorType.NotFound);
            return null;
        }

        var source = new Models.Source
        {
            Name = createSourceDto.Name,
            Amount = createSourceDto.Amount,
            UserId = userId,
            CurrencyId = createSourceDto.CurrencyId,
            IsArchived = false,
            Currency = currency
        };

        var result = context.Sources.Add(source);

        await context.SaveChangesAsync();
        return CreateSourceDto(result.Entity);
    }

    public async Task<SourceDto?> ArchiveSource(int sourceId, int userId)
    {
        var source = await context.Sources.Include(s => s.Currency)
            .FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        source.IsArchived = true;
        await context.SaveChangesAsync();

        return CreateSourceDto(source);
    }

    public async Task<SourceDto?> UnArchiveSource(int sourceId, int userId)
    {
        var source = await context.Sources.Include(s => s.Currency)
            .FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        source.IsArchived = false;

        await context.SaveChangesAsync();

        return CreateSourceDto(source);
    }

    public async Task<SourceDto?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto, int userId)
    {
        var source = await context.Sources.Include(s => s.Currency)
            .FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        if (updateSourceDto.Name != null)
            source.Name = updateSourceDto.Name;

        await context.SaveChangesAsync();

        return CreateSourceDto(source);
    }

    private static SourceDto CreateSourceDto(Models.Source source)
    {
        return new SourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Amount = source.Amount,
            UserId = source.UserId,
            CurrencyId = source.CurrencyId,
            CurrencyName = source.Currency.Name
        };
    }
}
