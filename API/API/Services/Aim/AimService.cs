using API.Domain.Calculator;
using API.Dtos;
using API.Extensions;
using API.Inputs;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Aim;

public class AimService(NotificationContext notificationContext, AppDbContext context, ICurrentUserContext currentUserContext, IAimProgressCalculator aimProgressCalculator) : IAimService
{
    public async Task<SourceDtoLookup?> AddSourceToAim(int aimId, int sourceId)
    {
        var userId = currentUserContext.RequiredUserId;
        var aim = context.Aims.FirstOrDefault(a => a.Id == aimId && a.UserId == userId);

        if (aim == null)
        {
            notificationContext.AddNotification("Aim not found", ErrorType.NotFound);
            return null;
        }

        var source = context.Sources.FirstOrDefault(s => s.Id == sourceId && s.UserId == userId);
        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        var sourceAimExists = context.SourceAims.Any(sa => sa.AimId == aimId && sa.SourceId == sourceId && sa.Aim.UserId == userId && sa.Source.UserId == userId);
        if (sourceAimExists)
        {
            notificationContext.AddNotification("Source already associated with aim", ErrorType.BadRequest);
            return null;
        }

        aim.SourceAims.Add(new Models.SourceAim { AimId = aimId, SourceId = sourceId });
        await context.SaveChangesAsync();

        return source.Adapt<SourceDtoLookup>();
    }


    public async Task<bool> RemoveSourceFromAim(int aimId, int sourceId)
    {
        var userId = currentUserContext.RequiredUserId;

        var sourceAim = context.SourceAims.FirstOrDefault(sa => sa.AimId == aimId && sa.SourceId == sourceId && sa.Aim.UserId == userId && sa.Source.UserId == userId);
        if (sourceAim == null)
        {
            notificationContext.AddNotification("Source not associated with aim", ErrorType.NotFound);
            return false;
        }

        context.SourceAims.Remove(sourceAim);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<AimDto?> CreateAim(CreateAimInput input)
    {
        var userId = currentUserContext.RequiredUserId;
        var aim = input.Adapt<Models.Aim>();
        aim.UserId = userId;

        var newAim = context.Aims.Add(aim);
        await context.SaveChangesAsync();

        return newAim.Entity.Adapt<AimDto>();
    }

    public async Task<bool> DeleteAim(int id)
    {
        var userId = currentUserContext.RequiredUserId;
        var aim = await context.Aims.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (aim == null)
        {
            notificationContext.AddNotification("Aim not found", ErrorType.NotFound);
            return false;
        }

        context.Aims.Remove(aim);

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<AimDto?> GetAim(int id)
    {
        var userId = currentUserContext.RequiredUserId;
        var aims = await context.Aims
        .Where(a => a.UserId == userId)
        .AsNoTracking()
        .ProjectToType<AimDto>()
        .ToListAsync();


        if (aims.FirstOrDefault(a => a.Id == id) == null)
        {
            notificationContext.AddNotification("Aim not found", ErrorType.NotFound);
            return null;
        }

        var aimWithProgress = await aimProgressCalculator.CalculateAimProgress(aims);

        return aimWithProgress.FirstOrDefault(a => a.Id == id);

    }

    public async Task<PaginatedResult<AimDto>> GetAims(GetAimsInput input)
    {
        var userId = currentUserContext.RequiredUserId;
        var aims = await context.Aims
        .AsNoTracking()
        .Where(a => a.UserId == userId)
        .FilterBySources(input.SourceIds)
        .FilterByClosed(input.ClosedOnly)
        .ApplySorting(input.SortBy, input.SortDescending)
        .ProjectToType<AimDto>()
        .Skip(input.Offset)
        .Take(input.Limit)  
        .ToListAsync();
        
        var calculatedAims = await aimProgressCalculator.CalculateAimProgress(aims);
        
        return new PaginatedResult<AimDto>
        {
            Data = calculatedAims,
            Meta = new PaginationMeta
            {
                TotalCount = aims.Count,
                Limit = input.Limit,
                Offset = input.Offset
            }
        };
    }

    public async Task<AimDto?> UpdateAim(int id, UpdateAimInput input)
    {
        var userId = currentUserContext.RequiredUserId;
        var aim = await context.Aims.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (aim == null)
        {
            notificationContext.AddNotification("Aim not found", ErrorType.NotFound);
            return null;
        }

        input.AdaptIgnoreNull(aim);

        await context.SaveChangesAsync();

        return aim.Adapt<AimDto>();
    }
}
