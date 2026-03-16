using API.Dtos;
using API.Inputs;
using API.Utils.Map;
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
    public async Task<SourceDto?> GetSourceById(int sourceId)
    {
        var userId = currentUserProvider.RequiredUserId;

        var source = await context.Sources
            .AsNoTracking()
            .Include(s => s.Currency)
            .Where(s => s.Id == sourceId && s.UserId == userId)
            .Select(s => s.Adapt<SourceDto>())
            .FirstOrDefaultAsync();

        return source;
    }

    public async Task<IReadOnlyCollection<SourceDto>> GetSources()
    {
        var userId = currentUserProvider.RequiredUserId;

        var result = await context.Sources
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ProjectToType<SourceDto>()
            .ToListAsync();

        return result;
    }

    public async Task<SourceDto?> CreateSource(CreateSourceInput createSourceDto)
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
        return result.Entity.Adapt<SourceDto>();
    }

    public async Task<SourceDto?> ArchiveSource(int sourceId)
    {
        var userId = currentUserProvider.RequiredUserId;
        var source = await context.Sources.Include(s => s.Currency)
            .FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }


        source.IsArchived = true;
        await context.SaveChangesAsync();

        return source.Adapt<SourceDto>();
    }

    public async Task<SourceDto?> UnArchiveSource(int sourceId)
    {
        var userId = currentUserProvider.RequiredUserId;
        var source = await context.Sources.Include(s => s.Currency)
            .FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        source.IsArchived = false;

        await context.SaveChangesAsync();

        return source.Adapt<SourceDto>();
    }

    public async Task<SourceDto?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto)
    {
        var userId = currentUserProvider.RequiredUserId;
        var source = await context.Sources.Include(s => s.Currency)
            .FirstOrDefaultAsync(s => s.Id == sourceId && s.UserId == userId);

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
            .ProjectToType<SourceDto>()
            .FirstOrDefaultAsync();

        return updatedSource;
    }
}
