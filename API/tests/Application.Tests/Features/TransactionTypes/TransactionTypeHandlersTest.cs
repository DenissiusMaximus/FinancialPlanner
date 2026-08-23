using FinancialPlanner.Application.Features.TransactionTypes.Queries.GetTransactionTypeById;
using FinancialPlanner.Application.Features.TransactionTypes.Queries.GetTransactionTypes;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.TransactionTypes;

public class TransactionTypeHandlersTest : BaseTest
{
    [Fact]
    public async Task GetTransactionTypes_ReturnsAll()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.TransactionTypes.AddRange(
            new TransactionType { Id = 1, Name = "Expense" },
            new TransactionType { Id = 2, Name = "Income" });
        await dbContext.SaveChangesAsync();

        var handler = new GetTransactionTypesQueryHandler(new TransactionTypeRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetTransactionTypesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTransactionTypeById_ReturnsNotFound_WhenNotFound()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetTransactionTypeByIdQueryHandler(new TransactionTypeRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetTransactionTypeByIdQuery(404), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TransactionTypeErrors.NotFound(404).Code);
    }

    [Fact]
    public async Task GetTransactionTypeById_ReturnsType_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.TransactionTypes.Add(new TransactionType { Id = 5, Name = "Transfer" });
        await dbContext.SaveChangesAsync();

        var handler = new GetTransactionTypeByIdQueryHandler(new TransactionTypeRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetTransactionTypeByIdQuery(5), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
    }
}
