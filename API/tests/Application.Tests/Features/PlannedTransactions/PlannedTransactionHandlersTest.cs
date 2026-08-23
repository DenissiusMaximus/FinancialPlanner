using FinancialPlanner.Application.Features.PlannedTransactions.Commands.CreatePlannedTransaction;
using FinancialPlanner.Application.Features.PlannedTransactions.Commands.DeletePlannedTransaction;
using FinancialPlanner.Application.Features.PlannedTransactions.Commands.UpdatePlannedTransaction;
using FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactionById;
using FinancialPlanner.Application.Features.PlannedTransactions.Queries.GetPlannedTransactions;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.PlannedTransactions;

public class PlannedTransactionHandlersTest : BaseTest
{
    [Fact]
    public async Task CreatePlannedTransaction_ReturnsCreatedTransaction()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        var handler = new CreatePlannedTransactionCommandHandler(
            new CreatePlannedTransactionCommandValidator(),
            new PlannedTransactionRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(userId),
            GetMapper());

        var command = new CreatePlannedTransactionCommand("Rent", 1000, new DateTime(2026, 1, 1), 1, 1, 1, 1, 1);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Rent");
        result.Value.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetPlannedTransactionById_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetPlannedTransactionByIdQueryHandler(new PlannedTransactionRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetPlannedTransactionByIdQuery(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PlannedTransactionErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task GetPlannedTransactionById_ReturnsTransaction_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.Add(new PlannedTransaction
        {
            Id = 3,
            Name = "Existing",
            Amount = 100,
            StartDate = new DateTime(2026, 1, 1),
            UserId = userId,
            CurrencyId = 1,
            TransactionTypeId = 1,
            SourceId = 1,
            FrequencyId = 1
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetPlannedTransactionByIdQueryHandler(new PlannedTransactionRepository(dbContext), GetMockUserContext(userId), GetMapper());

        var result = await handler.HandleAsync(new GetPlannedTransactionByIdQuery(3), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(3);
    }

    [Fact]
    public async Task GetPlannedTransactions_ReturnsOnlyCurrentUsers()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedDependencies(dbContext, 1);
        await SeedDependencies(dbContext, 2);

        dbContext.PlannedTransactions.AddRange(
            new PlannedTransaction { Id = 1, Name = "P1", Amount = 100, StartDate = new DateTime(2026, 1, 1), UserId = 1, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 },
            new PlannedTransaction { Id = 2, Name = "P2", Amount = 200, StartDate = new DateTime(2026, 1, 2), UserId = 2, CurrencyId = 1, TransactionTypeId = 1, SourceId = 2, FrequencyId = 2 });
        await dbContext.SaveChangesAsync();

        var handler = new GetPlannedTransactionsQueryHandler(new GetPlannedTransactionsQueryValidator(), new PlannedTransactionRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetPlannedTransactionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().ContainSingle();
        result.Value.Data.First().UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetPlannedTransactions_AppliesLimitAndOffset()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.AddRange(
            new PlannedTransaction { Id = 11, Name = "A", Amount = 100, StartDate = new DateTime(2026, 1, 1), UserId = userId, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 },
            new PlannedTransaction { Id = 12, Name = "B", Amount = 100, StartDate = new DateTime(2026, 1, 1), UserId = userId, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 },
            new PlannedTransaction { Id = 13, Name = "C", Amount = 100, StartDate = new DateTime(2026, 1, 1), UserId = userId, CurrencyId = 1, TransactionTypeId = 1, SourceId = 1, FrequencyId = 1 });
        await dbContext.SaveChangesAsync();

        var handler = new GetPlannedTransactionsQueryHandler(new GetPlannedTransactionsQueryValidator(), new PlannedTransactionRepository(dbContext), GetMockUserContext(userId), GetMapper());

        var result = await handler.HandleAsync(new GetPlannedTransactionsQuery(Limit: 1, Offset: 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().ContainSingle();
        result.Value.Meta.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task UpdatePlannedTransaction_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new UpdatePlannedTransactionCommandHandler(
            new UpdatePlannedTransactionCommandValidator(),
            new PlannedTransactionRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdatePlannedTransactionCommand(999, "Updated", null, null, null, null, null, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PlannedTransactionErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task UpdatePlannedTransaction_ReturnsUpdatedTransaction_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.Add(new PlannedTransaction
        {
            Id = 5,
            Name = "Old Name",
            Amount = 500,
            StartDate = new DateTime(2026, 1, 1),
            UserId = userId,
            CurrencyId = 1,
            TransactionTypeId = 1,
            SourceId = 1,
            FrequencyId = 1
        });
        await dbContext.SaveChangesAsync();

        var handler = new UpdatePlannedTransactionCommandHandler(
            new UpdatePlannedTransactionCommandValidator(),
            new PlannedTransactionRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(userId),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdatePlannedTransactionCommand(5, "New Name", 700, null, null, null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Amount.Should().Be(700);
    }

    [Fact]
    public async Task DeletePlannedTransaction_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new DeletePlannedTransactionCommandHandler(new PlannedTransactionRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeletePlannedTransactionCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PlannedTransactionErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task DeletePlannedTransaction_Succeeds_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedDependencies(dbContext, userId);

        dbContext.PlannedTransactions.Add(new PlannedTransaction
        {
            Id = 6,
            Name = "To Delete",
            Amount = 300,
            StartDate = new DateTime(2026, 1, 1),
            UserId = userId,
            CurrencyId = 1,
            TransactionTypeId = 1,
            SourceId = 1,
            FrequencyId = 1
        });
        await dbContext.SaveChangesAsync();

        var handler = new DeletePlannedTransactionCommandHandler(new PlannedTransactionRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(userId));

        var result = await handler.HandleAsync(new DeletePlannedTransactionCommand(6), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.PlannedTransactions.Any(t => t.Id == 6).Should().BeFalse();
    }

    private static async Task SeedDependencies(ApplicationDbContext dbContext, int userId)
    {
        if (!dbContext.Currencies.Any(c => c.Id == 1))
            dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });

        if (!dbContext.TransactionTypes.Any(t => t.Id == 1))
            dbContext.TransactionTypes.Add(new TransactionType { Id = 1, Name = "Expense" });

        if (!dbContext.IntervalUnits.Any(i => i.Id == userId))
            dbContext.IntervalUnits.Add(new IntervalUnit { Id = userId, Name = $"Unit{userId}" });

        if (!dbContext.Frequencies.Any(f => f.Id == userId))
            dbContext.Frequencies.Add(new Frequency { Id = userId, Name = $"Freq{userId}", IntervalValue = 1, IntervalUnitId = userId, UserId = userId });

        if (!dbContext.Sources.Any(s => s.Id == userId))
            dbContext.Sources.Add(new Source { Id = userId, Name = $"Source{userId}", Amount = 1000, UserId = userId, CurrencyId = 1, IsArchived = false });

        if (!dbContext.Categories.Any(c => c.Id == userId))
            dbContext.Categories.Add(new Category { Id = userId, Name = $"Category{userId}", UserId = userId });

        await dbContext.SaveChangesAsync();
    }
}
