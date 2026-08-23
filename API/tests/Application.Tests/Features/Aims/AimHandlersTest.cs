using FinancialPlanner.Application.Features.Aims.Commands.AddSourceToAim;
using FinancialPlanner.Application.Features.Aims.Commands.CreateAim;
using FinancialPlanner.Application.Features.Aims.Commands.DeleteAim;
using FinancialPlanner.Application.Features.Aims.Commands.RemoveSourceFromAim;
using FinancialPlanner.Application.Features.Aims.Commands.UpdateAim;
using FinancialPlanner.Application.Features.Aims.Queries.GetAim;
using FinancialPlanner.Application.Features.Aims.Queries.GetAims;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Services;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Aims;

public class AimHandlersTest : BaseTest
{
    [Fact]
    public async Task CreateAim_ReturnsCreatedAim()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var handler = new CreateAimCommandHandler(
            new CreateAimCommandValidator(),
            new AimRepository(dbContext),
            new CurrencyRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(testUserId),
            GetMapper());

        var command = new CreateAimCommand("Save for vacation", 1200, 2, null);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.Name.Should().Be(command.Name);
        result.Value.Amount.Should().Be(command.Amount);
        result.Value.Priority.Should().Be(command.Priority);
        result.Value.UserId.Should().Be(testUserId);
    }

    [Fact]
    public async Task GetAim_ReturnsAimById()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var aim = new Aim { Id = 10, Name = "Emergency fund", Amount = 5000, Priority = 1, UserId = testUserId, IsClosed = false };
        dbContext.Aims.Add(aim);
        await dbContext.SaveChangesAsync();

        var handler = new GetAimQueryHandler(new AimRepository(dbContext), new AimProgressCalculator(), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new GetAimQuery(aim.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(aim.Id);
        result.Value.Name.Should().Be(aim.Name);
        result.Value.UserId.Should().Be(testUserId);
    }

    [Fact]
    public async Task GetAim_ReturnsNotFound_WhenAimNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetAimQueryHandler(new AimRepository(dbContext), new AimProgressCalculator(), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetAimQuery(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task GetAims_ReturnsOnlyCurrentUserAims()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        dbContext.Aims.AddRange(
            new Aim { Id = 1, Name = "A1", Amount = 100, Priority = 1, UserId = testUserId, IsClosed = false },
            new Aim { Id = 2, Name = "A2", Amount = 200, Priority = 2, UserId = testUserId, IsClosed = false },
            new Aim { Id = 3, Name = "Other user aim", Amount = 300, Priority = 3, UserId = 2, IsClosed = false });
        await dbContext.SaveChangesAsync();

        var handler = new GetAimsQueryHandler(new GetAimsQueryValidator(), new AimRepository(dbContext), new AimProgressCalculator(), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new GetAimsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(2);
        result.Value.Data.Should().OnlyContain(a => a.UserId == testUserId);
    }

    [Fact]
    public async Task UpdateAim_ReturnsUpdatedAim()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var aim = new Aim { Id = 1, Name = "Old name", Amount = 1000, Priority = 1, UserId = testUserId, IsClosed = false };
        dbContext.Aims.Add(aim);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateAimCommandHandler(
            new UpdateAimCommandValidator(),
            new AimRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(testUserId),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateAimCommand(aim.Id, "Updated name", 1500, 3, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated name");
        result.Value.Amount.Should().Be(1500);
        result.Value.Priority.Should().Be(3);
    }

    [Fact]
    public async Task UpdateAim_ReturnsNotFound_WhenAimNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new UpdateAimCommandHandler(
            new UpdateAimCommandValidator(),
            new AimRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateAimCommand(999, "x", null, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task DeleteAim_Succeeds_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var aim = new Aim { Id = 1, Name = "To delete", Amount = 1000, Priority = 1, UserId = testUserId, IsClosed = false };
        dbContext.Aims.Add(aim);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteAimCommandHandler(new AimRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId));

        var result = await handler.HandleAsync(new DeleteAimCommand(aim.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.Aims.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAim_ReturnsNotFound_WhenAimNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new DeleteAimCommandHandler(new AimRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeleteAimCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task AddSourceToAim_ReturnsSourceWhenAdded()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1 };
        var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
        var source = new Source { Id = 2, Name = "Savings", Amount = 2000, UserId = testUserId, CurrencyId = currency.Id, Currency = currency, IsArchived = false };

        dbContext.Currencies.Add(currency);
        dbContext.Aims.Add(aim);
        dbContext.Sources.Add(source);
        await dbContext.SaveChangesAsync();

        var handler = new AddSourceToAimCommandHandler(new AimRepository(dbContext), new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new AddSourceToAimCommand(aim.Id, source.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(source.Id);
        dbContext.SourceAims.Should().ContainSingle(sa => sa.AimId == aim.Id && sa.SourceId == source.Id);
    }

    [Fact]
    public async Task AddSourceToAim_ReturnsNotFound_WhenAimNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        dbContext.Sources.Add(new Source { Id = 2, Name = "Savings", Amount = 2000, UserId = testUserId, CurrencyId = 1, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var handler = new AddSourceToAimCommandHandler(new AimRepository(dbContext), new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new AddSourceToAimCommand(999, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task AddSourceToAim_ReturnsNotFound_WhenSourceNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
        dbContext.Aims.Add(aim);
        await dbContext.SaveChangesAsync();

        var handler = new AddSourceToAimCommandHandler(new AimRepository(dbContext), new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new AddSourceToAimCommand(aim.Id, 999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SourceErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task AddSourceToAim_ReturnsConflict_WhenAssociationAlreadyExists()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
        var source = new Source { Id = 2, Name = "Savings", Amount = 2000, UserId = testUserId, CurrencyId = 1, Currency = currency, IsArchived = false };
        var sourceAim = new SourceAim { Id = 1, AimId = aim.Id, SourceId = source.Id, Aim = aim, Source = source };

        dbContext.Currencies.Add(currency);
        dbContext.Aims.Add(aim);
        dbContext.Sources.Add(source);
        dbContext.SourceAims.Add(sourceAim);
        await dbContext.SaveChangesAsync();

        var handler = new AddSourceToAimCommandHandler(new AimRepository(dbContext), new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new AddSourceToAimCommand(aim.Id, source.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.SourceAlreadyLinked(aim.Id, source.Id).Code);
    }

    [Fact]
    public async Task RemoveSourceFromAim_Succeeds_WhenAssociationExists()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;

        var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
        var source = new Source { Id = 2, Name = "Savings", Amount = 2000, UserId = testUserId, CurrencyId = 1, IsArchived = false };
        var sourceAim = new SourceAim { Id = 1, AimId = aim.Id, SourceId = source.Id, Aim = aim, Source = source };

        dbContext.Aims.Add(aim);
        dbContext.Sources.Add(source);
        dbContext.SourceAims.Add(sourceAim);
        await dbContext.SaveChangesAsync();

        var handler = new RemoveSourceFromAimCommandHandler(new AimRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId));

        var result = await handler.HandleAsync(new RemoveSourceFromAimCommand(aim.Id, source.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.SourceAims.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveSourceFromAim_ReturnsNotFound_WhenAimNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new RemoveSourceFromAimCommandHandler(new AimRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1));

        var result = await handler.HandleAsync(new RemoveSourceFromAimCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.NotFound(1).Code);
    }

    [Fact]
    public async Task RemoveSourceFromAim_ReturnsNotFound_WhenAssociationNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        var testUserId = 1;
        var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
        dbContext.Aims.Add(aim);
        await dbContext.SaveChangesAsync();

        var handler = new RemoveSourceFromAimCommandHandler(new AimRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId));

        var result = await handler.HandleAsync(new RemoveSourceFromAimCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AimErrors.SourceLinkNotFound(1, 2).Code);
    }
}
