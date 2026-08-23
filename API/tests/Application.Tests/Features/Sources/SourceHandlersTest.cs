using FinancialPlanner.Application.Features.Sources.Commands.ArchiveSource;
using FinancialPlanner.Application.Features.Sources.Commands.CreateSource;
using FinancialPlanner.Application.Features.Sources.Commands.UnarchiveSource;
using FinancialPlanner.Application.Features.Sources.Commands.UpdateSource;
using FinancialPlanner.Application.Features.Sources.Queries.GetSourceById;
using FinancialPlanner.Application.Features.Sources.Queries.GetSources;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Sources;

public class SourceHandlersTest : BaseTest
{
    [Fact]
    public async Task GetSources_ReturnsOnlyCurrentUserSources()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.AddRange(
            new Source { Id = 1, Name = "S1", Amount = 100, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false },
            new Source { Id = 2, Name = "S2", Amount = 200, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false },
            new Source { Id = 3, Name = "Other", Amount = 300, UserId = 2, CurrencyId = 1, Currency = currency, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var handler = new GetSourcesQueryHandler(new SourceRepository(dbContext), GetMockUserContext(userId), GetMapper());

        var result = await handler.HandleAsync(new GetSourcesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(x => x.UserId == userId);
    }

    [Fact]
    public async Task GetSourceById_ReturnsSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 7, Name = "Wallet", Amount = 500, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var handler = new GetSourceByIdQueryHandler(new SourceRepository(dbContext), GetMockUserContext(userId), GetMapper());

        var result = await handler.HandleAsync(new GetSourceByIdQuery(7), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(7);
    }

    [Fact]
    public async Task CreateSource_ReturnsNotFound_WhenCurrencyNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new CreateSourceCommandHandler(
            new CreateSourceCommandValidator(),
            new SourceRepository(dbContext),
            new CurrencyRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetMapper());

        var result = await handler.HandleAsync(new CreateSourceCommand("Cash", 100, 404), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CurrencyErrors.NotFound(404).Code);
    }

    [Fact]
    public async Task CreateSource_ReturnsCreatedSource_WhenCurrencyExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });
        await dbContext.SaveChangesAsync();

        var handler = new CreateSourceCommandHandler(
            new CreateSourceCommandValidator(),
            new SourceRepository(dbContext),
            new CurrencyRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetMapper());

        var result = await handler.HandleAsync(new CreateSourceCommand("Cash", 100, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Cash");
        result.Value.UserId.Should().Be(1);
    }

    [Fact]
    public async Task ArchiveSource_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new ArchiveSourceCommandHandler(new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new ArchiveSourceCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SourceErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task ArchiveSource_ReturnsUpdatedSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 8, Name = "Wallet", Amount = 500, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var handler = new ArchiveSourceCommandHandler(new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(userId), GetMapper());

        var result = await handler.HandleAsync(new ArchiveSourceCommand(8), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task UnarchiveSource_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new UnarchiveSourceCommandHandler(new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new UnarchiveSourceCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SourceErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task UpdateSource_ReturnsUpdatedSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 4, Name = "Old", Amount = 99, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var handler = new UpdateSourceCommandHandler(
            new UpdateSourceCommandValidator(),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(userId),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateSourceCommand(4, "New"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New");
    }

    [Fact]
    public async Task UpdateSource_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new UpdateSourceCommandHandler(
            new UpdateSourceCommandValidator(),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateSourceCommand(999, "New"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SourceErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task UnarchiveSource_ReturnsUpdatedSource_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;

        var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m };
        dbContext.Currencies.Add(currency);
        dbContext.Sources.Add(new Source { Id = 9, Name = "Wallet", Amount = 500, UserId = userId, CurrencyId = 1, Currency = currency, IsArchived = true });
        await dbContext.SaveChangesAsync();

        var handler = new UnarchiveSourceCommandHandler(new SourceRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(userId), GetMapper());

        var result = await handler.HandleAsync(new UnarchiveSourceCommand(9), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsArchived.Should().BeFalse();
    }
}
