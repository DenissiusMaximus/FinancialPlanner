using FinancialPlanner.Application.Features.Currencies.Queries.GetCurrencies;
using FinancialPlanner.Application.Features.Currencies.Queries.GetCurrencyById;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Currencies;

public class CurrencyHandlersTest : BaseTest
{
    [Fact]
    public async Task GetCurrencies_ReturnsAllCurrencies()
    {
        var dbContext = GetInMemoryDbContext();

        dbContext.Currencies.AddRange(
            new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1m },
            new Currency { Id = 2, Name = "EUR", UsdExchangeRate = 1.1m });
        await dbContext.SaveChangesAsync();

        var handler = new GetCurrenciesQueryHandler(new CurrencyRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetCurrenciesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCurrencyById_ReturnsCurrency_WhenExists()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.Currencies.Add(new Currency { Id = 10, Name = "UAH", UsdExchangeRate = 0.025m });
        await dbContext.SaveChangesAsync();

        var handler = new GetCurrencyByIdQueryHandler(new CurrencyRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetCurrencyByIdQuery(10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);
        result.Value.Name.Should().Be("UAH");
    }

    [Fact]
    public async Task GetCurrencyById_ReturnsNotFound_WhenNotExists()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetCurrencyByIdQueryHandler(new CurrencyRepository(dbContext), GetMapper());

        var result = await handler.HandleAsync(new GetCurrencyByIdQuery(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CurrencyErrors.NotFound(999).Code);
    }
}
