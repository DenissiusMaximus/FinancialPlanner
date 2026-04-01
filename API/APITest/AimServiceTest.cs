using API.Domain.Calculator;
using API.Models;
using API.Services.Aim;
using API.Utils.Notification;

namespace APITest;

public class AimServiceTest : BaseTest
{
    [Fact]
    public async Task CalculateAimProgress_ReturnsCorrectProgress()
    {
        var source1 = new Source { Id = 1, Name = "Source 1", Amount = 1000, UserId = 1 };
        var source2 = new Source { Id = 2, Name = "Source 2", Amount = 2000, UserId = 1 };
        var aim = new Aim { Id = 1, Amount = 2500, Name = "Aim 1", UserId = 1 };
        var aim2 = new Aim { Id = 2, Amount = 5000, Name = "Aim 2", UserId = 1 };

        var sourceAim1 = new SourceAim { AimId = aim.Id, SourceId = source1.Id };
        var sourceAim2 = new SourceAim { AimId = aim.Id, SourceId = source2.Id };
        var sourceAim3 = new SourceAim { AimId = aim2.Id, SourceId = source1.Id };
        var sourceAim4 = new SourceAim { AimId = aim2.Id, SourceId = source2.Id };

        aim.SourceAims = new List<SourceAim> { sourceAim1, sourceAim2 };
        aim2.SourceAims = new List<SourceAim> { sourceAim3, sourceAim4 };

        var dbContext = GetInMemoryDbContext();
        dbContext.Sources.AddRange(source1, source2);
        dbContext.Aims.AddRange(aim, aim2);
        dbContext.SourceAims.AddRange(sourceAim1, sourceAim2, sourceAim3, sourceAim4);
        await dbContext.SaveChangesAsync();

        var mockUserContext = GetMockUserContext(1);
        var notificationContext = new NotificationContext();
        var aimProgressCalculator = new AimProgressCalculator();

        var aimService = new AimService(notificationContext, dbContext, mockUserContext, aimProgressCalculator);

        var result = await aimService.GetAims();
        var aims = dbContext.Aims.ToList();
    }
}