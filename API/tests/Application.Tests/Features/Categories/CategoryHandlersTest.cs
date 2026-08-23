using FinancialPlanner.Application.Features.Categories.Commands.CreateCategory;
using FinancialPlanner.Application.Features.Categories.Commands.DeleteCategory;
using FinancialPlanner.Application.Features.Categories.Commands.UpdateCategory;
using FinancialPlanner.Application.Features.Categories.Queries.GetCategories;
using FinancialPlanner.Application.Features.Categories.Queries.GetCategoryById;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Errors;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FluentAssertions;

namespace FinancialPlanner.Application.Tests.Features.Categories;

public class CategoryHandlersTest : BaseTest
{
    [Fact]
    public async Task GetCategories_ReturnsAllCategoriesForUser()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var otherUserId = 2;
        var testCategoryIds = new[] { 1, 2, 3 };

        foreach (var categoryId in testCategoryIds)
            dbContext.Categories.Add(new Category { Id = categoryId, Name = $"Test Category {categoryId}", UserId = testUserId });

        dbContext.Categories.Add(new Category { Id = 99, Name = "Other user category", UserId = otherUserId });

        await dbContext.SaveChangesAsync();

        var handler = new GetCategoriesQueryHandler(new CategoryRepository(dbContext), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new GetCategoriesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(testCategoryIds.Length);
        result.Value.Should().OnlyContain(c => c.UserId == testUserId);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsCategory()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var testCategoryId = 100;

        dbContext.Categories.Add(new Category { Id = testCategoryId, Name = $"Test Category {testCategoryId}", UserId = testUserId });
        await dbContext.SaveChangesAsync();

        var handler = new GetCategoryByIdQueryHandler(new CategoryRepository(dbContext), GetMockUserContext(testUserId), GetMapper());

        var result = await handler.HandleAsync(new GetCategoryByIdQuery(testCategoryId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(testCategoryId);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotFound_ForNonExistentId()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetCategoryByIdQueryHandler(new CategoryRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetCategoryByIdQuery(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CategoryErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task GetCategories_ReturnsEmptyList_ForUserWithNoCategories()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new GetCategoriesQueryHandler(new CategoryRepository(dbContext), GetMockUserContext(1), GetMapper());

        var result = await handler.HandleAsync(new GetCategoriesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreatedCategory()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var command = new CreateCategoryCommand("New Category");

        var handler = new CreateCategoryCommandHandler(
            new CreateCategoryCommandValidator(),
            new CategoryRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(testUserId),
            GetMapper());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.Name.Should().Be(command.Name);
        result.Value.UserId.Should().Be(testUserId);
    }

    [Fact]
    public async Task CreateCategory_ReturnsValidationError_ForEmptyName()
    {
        var dbContext = GetInMemoryDbContext();

        var command = new CreateCategoryCommand("");

        var handler = new CreateCategoryCommandHandler(
            new CreateCategoryCommandValidator(),
            new CategoryRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetMapper());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(FinancialPlanner.Domain.Common.ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsUpdatedCategory()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var category = new Category { Id = 1, Name = "Old Category", UserId = testUserId };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCategoryCommandHandler(
            new UpdateCategoryCommandValidator(),
            new CategoryRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(testUserId),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateCategoryCommand(category.Id, "Updated Category"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be("Updated Category");
        result.Value.UserId.Should().Be(testUserId);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsNotFound_ForNonExistentId()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new UpdateCategoryCommandHandler(
            new UpdateCategoryCommandValidator(),
            new CategoryRepository(dbContext),
            new UnitOfWork(dbContext),
            GetMockUserContext(1),
            GetPatchMapper(),
            GetMapper());

        var result = await handler.HandleAsync(new UpdateCategoryCommand(999, "Updated Category"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CategoryErrors.NotFound(999).Code);
    }

    [Fact]
    public async Task DeleteCategory_DeletesCategory()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var category = new Category { Id = 1, Name = "Test Category", UserId = testUserId };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteCategoryCommandHandler(new CategoryRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(testUserId));

        var result = await handler.HandleAsync(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var deletedCategory = await dbContext.Categories.FindAsync(category.Id);
        deletedCategory.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNotFound_ForNonExistentId()
    {
        var dbContext = GetInMemoryDbContext();

        var handler = new DeleteCategoryCommandHandler(new CategoryRepository(dbContext), new UnitOfWork(dbContext), GetMockUserContext(1));

        var result = await handler.HandleAsync(new DeleteCategoryCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CategoryErrors.NotFound(999).Code);
    }
}
