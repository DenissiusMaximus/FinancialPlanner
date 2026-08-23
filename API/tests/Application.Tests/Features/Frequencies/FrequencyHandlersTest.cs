using FinancialPlanner.Application.Features.Frequencies.Commands.CreateFrequency;
using FinancialPlanner.Application.Features.Frequencies.Commands.DeleteFrequency;
using FinancialPlanner.Application.Features.Frequencies.Commands.UpdateFrequency;
using FinancialPlanner.Application.Features.Frequencies.Queries.GetFrequencies;
using FinancialPlanner.Application.Features.Frequencies.Queries.GetFrequencyById;
using FinancialPlanner.Application.Features.Frequencies.Queries.GetUserFrequencies;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Frequencies;

public class FrequencyHandlersTest : BaseTest
{
    [Fact]
    public async Task CreateFrequency_ReturnsNotFound_WhenIntervalUnitNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new CreateFrequencyCommandHandler(
            new CreateFrequencyCommandValidator(),
            new FrequencyRepository(dbContext),
            new IntervalUnitRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetMapper());

        var result = await handler.HandleAsync(new CreateFrequencyCommand("Weekly", 999, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(FrequencyErrors.IntervalUnitNotFound(999).Code);
    }

    [Fact]
    public async Task CreateFrequency_ReturnsCreatedFrequency()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        await dbContext.SaveChangesAsync();

        var handler = new CreateFrequencyCommandHandler(
            new CreateFrequencyCommandValidator(),
            new FrequencyRepository(dbContext),
            new IntervalUnitRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetMapper());

        var result = await handler.HandleAsync(new CreateFrequencyCommand("Weekly", 1, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Weekly");
        result.Value.UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetFrequencies_ReturnsUserAndSharedFrequencies()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.AddRange(
            new Frequency { Id = 1, Name = "Shared", IntervalValue = 1, IntervalUnitId = 1, UserId = null },
            new Frequency { Id = 2, Name = "Mine", IntervalValue = 2, IntervalUnitId = 1, UserId = 1 },
            new Frequency { Id = 3, Name = "Other", IntervalValue = 3, IntervalUnitId = 1, UserId = 2 });
        await dbContext.SaveChangesAsync();

        var handler = new GetFrequenciesQueryHandler(new FrequencyRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetFrequenciesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(x => x.Id).Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task GetFrequencyById_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetFrequencyByIdQueryHandler(new FrequencyRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetFrequencyByIdQuery(777), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(FrequencyErrors.NotFound(777).Code);
    }

    [Fact]
    public async Task GetFrequencyById_ReturnsFrequency_WhenExistsForUser()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.Add(new Frequency { Id = 10, Name = "UserFreq", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 });
        await dbContext.SaveChangesAsync();

        var handler = new GetFrequencyByIdQueryHandler(new FrequencyRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetFrequencyByIdQuery(10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);
    }

    [Fact]
    public async Task DeleteFrequency_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new DeleteFrequencyCommandHandler(new FrequencyRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeleteFrequencyCommand(777), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(FrequencyErrors.NotFound(777).Code);
    }

    [Fact]
    public async Task DeleteFrequency_Succeeds_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.Add(new Frequency { Id = 12, Name = "ToDelete", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 });
        await dbContext.SaveChangesAsync();

        var handler = new DeleteFrequencyCommandHandler(new FrequencyRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeleteFrequencyCommand(12), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.Frequencies.Any(f => f.Id == 12).Should().BeFalse();
    }

    [Fact]
    public async Task GetUserFrequencies_ReturnsOnlyUsersFrequencies()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.AddRange(
            new Frequency { Id = 20, Name = "Mine", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 },
            new Frequency { Id = 21, Name = "Other", IntervalValue = 1, IntervalUnitId = 1, UserId = 2 },
            new Frequency { Id = 22, Name = "Shared", IntervalValue = 1, IntervalUnitId = 1, UserId = null });
        await dbContext.SaveChangesAsync();

        var handler = new GetUserFrequenciesQueryHandler(new FrequencyRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetUserFrequenciesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.First().Id.Should().Be(20);
    }

    [Fact]
    public async Task UpdateFrequency_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new UpdateFrequencyCommandHandler(
            new UpdateFrequencyCommandValidator(),
            new FrequencyRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateFrequencyCommand(404, "new", null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(FrequencyErrors.NotFound(404).Code);
    }

    [Fact]
    public async Task UpdateFrequency_ReturnsUpdatedFrequency()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.IntervalUnits.Add(new IntervalUnit { Id = 1, Name = "Week" });
        dbContext.Frequencies.Add(new Frequency { Id = 11, Name = "Old", IntervalValue = 1, IntervalUnitId = 1, UserId = 1 });
        await dbContext.SaveChangesAsync();

        var handler = new UpdateFrequencyCommandHandler(
            new UpdateFrequencyCommandValidator(),
            new FrequencyRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateFrequencyCommand(11, "New", null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New");
    }
}
