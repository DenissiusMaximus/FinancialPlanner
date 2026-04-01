using API;
using API.Extensions;
using API.Utils.UserContext;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace APITest;

public class BaseTest
{
    static BaseTest()
    {
        MapConfig.Configure();
    }

    protected BaseTest()
    {
    }

    protected AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    protected ICurrentUserContext GetMockUserContext(int userId)
    {
        var mockUserContext = new Mock<ICurrentUserContext>();
        mockUserContext.Setup(uc => uc.RequiredUserId).Returns(userId);
        return mockUserContext.Object;
    }
}
