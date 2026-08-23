using FinancialPlanner.Application.Features.Transactions.Commands.CreateTransaction;
using FinancialPlanner.Application.Features.Transactions.Commands.DeleteTransaction;
using FinancialPlanner.Application.Features.Transactions.Commands.UpdateTransaction;
using FinancialPlanner.Application.Features.Transactions.Queries.GetTransactionById;
using FinancialPlanner.Application.Features.Transactions.Queries.GetTransactions;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Domain.Services;
using FinancialPlanner.Infrastructure.Database;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Transactions;

public class TransactionHandlersTest : BaseTest
{
    [Fact]
    public async Task CreateTransaction_Transfer_UpdatesBothSourceBalances()
    {
        var dbContext = GetInMemoryDbContext();
        var userId = 1;
        await SeedTransactionDependencies(dbContext, userId);
        dbContext.Sources.Add(new Source { Id = 11, Name = "Destination", Amount = 200, UserId = userId, CurrencyId = 1, IsArchived = false });
        await dbContext.SaveChangesAsync();

        var handler = new CreateTransactionCommandHandler(
            new CreateTransactionCommandValidator(),
            new TransactionRepository(dbContext),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            new BalanceManager(),
            GetMockUserContext(userId),
            GetMapper());

        var command = new CreateTransactionCommand(100, null, new DateTime(2026, 1, 15), null, 1, 11, 1, (int)TransactionTypeEnum.Transfer);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var source = await dbContext.Sources.FindAsync(1);
        var destination = await dbContext.Sources.FindAsync(11);
        source!.Amount.Should().Be(900);
        destination!.Amount.Should().Be(300);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsNotFound_WhenSourceMissing()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });
        dbContext.TransactionTypes.Add(new TransactionType { Id = 1, Name = "Expense" });
        await dbContext.SaveChangesAsync();

        var handler = new CreateTransactionCommandHandler(
            new CreateTransactionCommandValidator(),
            new TransactionRepository(dbContext),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            new BalanceManager(),
            GetMockUserContext(1),
            GetMapper());

        var command = new CreateTransactionCommand(50, null, new DateTime(2026, 2, 1), null, 999, null, 1, 1);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SourceErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsDestinationNotFound_WhenTransferDestinationMissing()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        var handler = new CreateTransactionCommandHandler(
            new CreateTransactionCommandValidator(),
            new TransactionRepository(dbContext),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            new BalanceManager(),
            GetMockUserContext(1),
            GetMapper());

        var command = new CreateTransactionCommand(50, null, new DateTime(2026, 2, 1), null, 1, 999, 1, (int)TransactionTypeEnum.Transfer);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SourceErrors.DestinationNotFound(999).Code);

        dbContext.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTransaction_Fails_ForAdjustmentTransaction()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        dbContext.Transactions.Add(new Transaction { Id = 20, Amount = 100, Date = new DateTime(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = (int)TransactionTypeEnum.Adjustment });
        await dbContext.SaveChangesAsync();

        var handler = new DeleteTransactionCommandHandler(new TransactionRepository(dbContext), new SourceRepository(dbContext), new UnitOfWork(dbContext), new BalanceManager(), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeleteTransactionCommand(20), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionErrors.AdjustmentNotDeletable.Code);
        dbContext.Transactions.Should().ContainSingle(t => t.Id == 20);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsNotFound_WhenTransactionNotFound()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        var handler = new DeleteTransactionCommandHandler(new TransactionRepository(dbContext), new SourceRepository(dbContext), new UnitOfWork(dbContext), new BalanceManager(), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeleteTransactionCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task DeleteTransaction_RevertsBalance_ForExpense()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        dbContext.Transactions.Add(new Transaction { Id = 21, Amount = 100, Date = new DateTime(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = (int)TransactionTypeEnum.Expense });
        await dbContext.SaveChangesAsync();

        var handler = new DeleteTransactionCommandHandler(new TransactionRepository(dbContext), new SourceRepository(dbContext), new UnitOfWork(dbContext), new BalanceManager(), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeleteTransactionCommand(21), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.Transactions.Any(t => t.Id == 21).Should().BeFalse();

        var source = await dbContext.Sources.FindAsync(1);
        source!.Amount.Should().Be(1100);
    }

    [Fact]
    public async Task UpdateTransaction_RecalculatesBalance_WhenAmountChanged()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        (await dbContext.Sources.FindAsync(1))!.Amount = 900;
        dbContext.Transactions.Add(new Transaction { Id = 30, Amount = 100, Date = new DateTime(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = (int)TransactionTypeEnum.Expense });
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTransactionCommandHandler(
            new UpdateTransactionCommandValidator(),
            new TransactionRepository(dbContext),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            new BalanceManager(),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateTransactionCommand(30, 250, null, null, null, null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(250);

        var source = await dbContext.Sources.FindAsync(1);
        source!.Amount.Should().Be(750);
    }

    [Fact]
    public async Task UpdateTransaction_DoesNotRecalculateBalance_WhenOnlyCommentChanged()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        dbContext.Transactions.Add(new Transaction { Id = 32, Amount = 100, Date = new DateTime(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = (int)TransactionTypeEnum.Expense, Comment = "old" });
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTransactionCommandHandler(
            new UpdateTransactionCommandValidator(),
            new TransactionRepository(dbContext),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            new BalanceManager(),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateTransactionCommand(32, null, "new", null, null, null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Comment.Should().Be("new");

        var source = await dbContext.Sources.FindAsync(1);
        source!.Amount.Should().Be(1000);
    }

    [Fact]
    public async Task UpdateTransaction_Fails_ForAdjustmentTransaction()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        dbContext.Transactions.Add(new Transaction { Id = 31, Amount = 100, Date = new DateTime(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = (int)TransactionTypeEnum.Adjustment });
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTransactionCommandHandler(
            new UpdateTransactionCommandValidator(),
            new TransactionRepository(dbContext),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            new BalanceManager(),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateTransactionCommand(31, 123, null, null, null, null, null, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionErrors.AdjustmentNotUpdatable.Code);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsNotFound_WhenTransactionMissing()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);

        var handler = new UpdateTransactionCommandHandler(
            new UpdateTransactionCommandValidator(),
            new TransactionRepository(dbContext),
            new SourceRepository(dbContext),
            new UnitOfWork(dbContext),
            new BalanceManager(),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateTransactionCommand(999, 123, null, null, null, null, null, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsTransaction_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        dbContext.Transactions.Add(new Transaction { Id = 1, Amount = 50, Date = new DateTime(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 });
        await dbContext.SaveChangesAsync();

        var handler = new GetTransactionByIdQueryHandler(new TransactionRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetTransactionByIdQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsNotFound_WhenMissing()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetTransactionByIdQueryHandler(new TransactionRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetTransactionByIdQuery(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task GetTransactions_FiltersByDateAndSort()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        dbContext.Transactions.AddRange(
            new Transaction { Id = 1, Amount = 100, Date = new DateTime(2026, 1, 2), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 },
            new Transaction { Id = 2, Amount = 300, Date = new DateTime(2026, 1, 3), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 },
            new Transaction { Id = 3, Amount = 200, Date = new DateTime(2026, 1, 1), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 },
            new Transaction { Id = 4, Amount = 999, Date = new DateTime(2026, 1, 4), UserId = 2, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1 });
        await dbContext.SaveChangesAsync();

        var handler = new GetTransactionsQueryHandler(new GetTransactionsQueryValidator(), new TransactionRepository(dbContext), GetMockUserContext(1), GetMapper());

        var query = new GetTransactionsQuery(
            FromDate: new DateOnly(2026, 1, 2),
            ToDate: new DateOnly(2026, 1, 3),
            SortBy: TransactionSortBy.Amount,
            SortDescending: true);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(2);
        result.Value.Data.First().Amount.Should().Be(300);
        result.Value.Data.Last().Amount.Should().Be(100);
    }

    [Fact]
    public async Task GetTransactions_FiltersByCategory_AndTotalCountRespectsFilter()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedTransactionDependencies(dbContext, 1);
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Food", UserId = 1 },
            new Category { Id = 2, Name = "Transport", UserId = 1 });
        dbContext.Transactions.AddRange(
            new Transaction { Id = 40, Amount = 100, Date = new DateTime(2026, 1, 2), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1, CategoryId = 1 },
            new Transaction { Id = 41, Amount = 200, Date = new DateTime(2026, 1, 3), UserId = 1, SourceId = 1, CurrencyId = 1, TransactionTypeId = 1, CategoryId = 2 });
        await dbContext.SaveChangesAsync();

        var handler = new GetTransactionsQueryHandler(new GetTransactionsQueryValidator(), new TransactionRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetTransactionsQuery(CategoryId: 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().ContainSingle();
        result.Value.Data.First().Category!.Id.Should().Be(1);
        result.Value.Meta.TotalCount.Should().Be(1);
    }

    private static async Task SeedTransactionDependencies(ApplicationDbContext dbContext, int userId)
    {
        if (!dbContext.Currencies.Any(c => c.Id == 1))
            dbContext.Currencies.Add(new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m });

        if (!dbContext.TransactionTypes.Any(t => t.Id == 1))
            dbContext.TransactionTypes.Add(new TransactionType { Id = 1, Name = "Expense" });

        if (!dbContext.TransactionTypes.Any(t => t.Id == (int)TransactionTypeEnum.Transfer))
            dbContext.TransactionTypes.Add(new TransactionType { Id = (int)TransactionTypeEnum.Transfer, Name = "Transfer" });

        if (!dbContext.TransactionTypes.Any(t => t.Id == (int)TransactionTypeEnum.Adjustment))
            dbContext.TransactionTypes.Add(new TransactionType { Id = (int)TransactionTypeEnum.Adjustment, Name = "Adjustment" });

        if (!dbContext.Sources.Any(s => s.Id == userId))
            dbContext.Sources.Add(new Source { Id = userId, Name = $"Source{userId}", Amount = 1000, UserId = userId, CurrencyId = 1, IsArchived = false });

        await dbContext.SaveChangesAsync();
    }
}
