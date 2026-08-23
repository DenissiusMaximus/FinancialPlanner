using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Mapping;
using FinancialPlanner.Infrastructure.Database;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinancialPlanner.Application.Tests;

public class BaseTest
{
    static BaseTest()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(IPasswordHasher).Assembly);
        PatchMapperConfig.Configure();
    }

    protected ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    protected static ICurrentUserContext GetMockUserContext(int userId)
    {
        var mockUserContext = new Mock<ICurrentUserContext>();
        mockUserContext.Setup(uc => uc.RequiredUserId).Returns(userId);
        return mockUserContext.Object;
    }

    protected static IMapper GetMapper() => new ServiceMapper(new ServiceCollection().BuildServiceProvider(), TypeAdapterConfig.GlobalSettings);

    protected static IPatchMapper GetPatchMapper() => new PatchMapper();
}
