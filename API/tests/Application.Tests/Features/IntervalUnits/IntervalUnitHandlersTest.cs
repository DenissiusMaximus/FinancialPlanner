using FinancialPlanner.Application.Features.IntervalUnits.Queries.GetIntervalUnitById;
using FinancialPlanner.Application.Features.IntervalUnits.Queries.GetIntervalUnits;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.IntervalUnits;

public class IntervalUnitHandlersTest : BaseTest
{
    [Fact]
    public async Task GetIntervalUnits_ReturnsAllUnits()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.AddRange(
            new IntervalUnit { Id = 1, Name = "Day" },
            new IntervalUnit { Id = 2, Name = "Month" });
        await dbContext.SaveChangesAsync();

        var handler = new GetIntervalUnitsQueryHandler(new IntervalUnitRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetIntervalUnitsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetIntervalUnitById_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetIntervalUnitByIdQueryHandler(new IntervalUnitRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetIntervalUnitByIdQuery(123), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IntervalUnitErrors.NotFound(123).Code);
    }

    [Fact]
    public async Task GetIntervalUnitById_ReturnsUnit_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 10, Name = "Week" });
        await dbContext.SaveChangesAsync();

        var handler = new GetIntervalUnitByIdQueryHandler(new IntervalUnitRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetIntervalUnitByIdQuery(10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);
    }
}
