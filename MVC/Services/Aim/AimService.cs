using API.Domain.Calculator;
using API.Extensions;
using API.Inputs;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Aim;

public class AimService(NotificationContext notificationContext, AppDbContext context, ICurrentUserContext currentUserContext, IAimProgressCalculator aimProgressCalculator) : IAimService
{
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

        return aims.FirstOrDefault(a => a.Id == id);

    }

    public async Task<IReadOnlyCollection<AimDto>> GetAims()
    {
        var userId = currentUserContext.RequiredUserId;
        var aims = await context.Aims
        .Where(a => a.UserId == userId)
        .ProjectToType<AimDto>()
        .ToListAsync();

        return await aimProgressCalculator.CalculateAimProgress(aims);
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
