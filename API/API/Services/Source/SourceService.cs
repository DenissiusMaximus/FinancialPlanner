using API.Dtos;
using API.Extensions;
using API.Inputs;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Source;

public class SourceService(
    AppDbContext context,
    NotificationContext notificationContext,
    ICurrentUserContext currentUserProvider) : ISourceService
{
    public async Task<SourceSummaryDto> GetSourceSummary()
    {
        var userId = currentUserProvider.RequiredUserId;

        var result = await context.Sources
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ProjectToType<SourceDtoLookup>()
            .ToListAsync();

        return new SourceSummaryDto
        {
            Total = result.Sum(s => s.Amount),
            Sources = result
        };
    }
    
    public async Task<SourceDtoDetailed?> GetSourceById(int sourceId)
    {
        var userId = currentUserProvider.RequiredUserId;

        var source = await context.Sources
            .AsNoTracking()
            .ProjectToType<SourceDtoDetailed>()
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .FirstOrDefaultAsync();

        return source;
    }

    public async Task<IReadOnlyCollection<SourceDtoLookup>> GetSources()
    {
        var userId = currentUserProvider.RequiredUserId;

        var result = await context.Sources
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ProjectToType<SourceDtoLookup>()
            .ToListAsync();

        return result;
    }

    public async Task<SourceDtoLookup?> CreateSource(CreateSourceInput createSourceDto)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == createSourceDto.CurrencyId);

        if (currency == null)
        {
            notificationContext.AddNotification("Currency not found", ErrorType.NotFound);
            return null;
        }

        var userId = currentUserProvider.RequiredUserId;

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
        return result.Entity.Adapt<SourceDtoLookup>();
    }

    public async Task<SourceDtoLookup?> ArchiveSource(int sourceId)
    {
        var userId = currentUserProvider.RequiredUserId;
        var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }


        source.IsArchived = true;
        await context.SaveChangesAsync();

        var updatedSource = await context.Sources
            .AsNoTracking()
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .ProjectToType<SourceDtoLookup>()
            .FirstOrDefaultAsync();

        return updatedSource;
    }

    public async Task<SourceDtoLookup?> UnArchiveSource(int sourceId)
    {
        var userId = currentUserProvider.RequiredUserId;
        var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        source.IsArchived = false;

        await context.SaveChangesAsync();

        var updatedSource = await context.Sources
            .AsNoTracking()
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .ProjectToType<SourceDtoLookup>()
            .FirstOrDefaultAsync();

        return updatedSource;
    }

    public async Task<SourceDtoLookup?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto)
    {
        var userId = currentUserProvider.RequiredUserId;
        var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        updateSourceDto.AdaptIgnoreNull(source);

        await context.SaveChangesAsync();

        var updatedSource = await context.Sources
            .AsNoTracking()
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .ProjectToType<SourceDtoLookup>()
            .FirstOrDefaultAsync();

        return updatedSource;
    }

    public Task<bool> DeleteSource(int sourceId)
    {
        var userId = currentUserProvider.RequiredUserId;
        var source = context.Sources.FirstOrDefault(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return Task.FromResult(false);
        }

        context.Sources.Remove(source);
        context.SaveChanges();

        return Task.FromResult(true);
    }
}
