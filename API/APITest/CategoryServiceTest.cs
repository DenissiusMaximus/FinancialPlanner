using API.Inputs;
using API.Models;
using API.Services.Category;
using API.Utils.Notification;
using FluentAssertions;

namespace APITest;

public class CategoryServiceTest : BaseTest
{
    public CategoryServiceTest() : base()
    {

    }

    [Fact]
    public async Task GetAllCategories_ReturnsAllCategories()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var testCategoryIds = new[] { 1, 2, 3 };

        foreach (var categoryId in testCategoryIds)
        {
            dbContext.Categories.Add(new Category { Id = categoryId, Name = $"Test Category {categoryId}", UserId = testUserId });
        }

        await dbContext.SaveChangesAsync();

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        var result = await categoryService.GetCategories();

        result.Should().NotBeNull();
        result.Should().HaveCount(testCategoryIds.Length);

        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task GetCategory_ReturnsCategoryById()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var testCategoryIds = 100;

        dbContext.Categories.Add(new Category { Id = testCategoryIds, Name = $"Test Category {testCategoryIds}", UserId = testUserId });

        await dbContext.SaveChangesAsync();

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        var result = await categoryService.GetCategoryById(testCategoryIds);

        result.Should().NotBeNull();
        result.Id.Should().Be(testCategoryIds);

        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task GetCategory_ReturnsNullForNonExistentId()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var nonExistentCategoryId = 999;

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        var result = await categoryService.GetCategoryById(nonExistentCategoryId);

        result.Should().BeNull();


        notificationContext.HasNotifications.Should().BeTrue();
        notificationContext.Notifications.Should().ContainSingle()
            .Which.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetCategories_ReturnsEmptyListForUserWithNoCategories()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        var result = await categoryService.GetCategories();

        result.Should().NotBeNull();
        result.Should().BeEmpty();

        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreatedCategory()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var categoryInput = new CreateCategoryInput { Name = "New Category", };

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        var result = await categoryService.CreateCategory(categoryInput);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(categoryInput.Name);
        result.UserId.Should().Be(testUserId);

        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCategory_ReturnsErrorForInvalidInput()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var categoryInput = new CreateCategoryInput { Name = "" };

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        var result = await categoryService.CreateCategory(categoryInput);

        result.Should().BeNull();

        notificationContext.HasNotifications.Should().BeTrue();
        notificationContext.Notifications.Should().ContainSingle()
            .Which.ErrorCode.Should().Be(ErrorType.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsUpdatedCategory()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var category = new Category { Id = 1, Name = "Old Category", UserId = testUserId };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var updateInput = new UpdateCategoryInput { Name = "Updated Category" };

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        var result = await categoryService.UpdateCategory(category.Id, updateInput);

        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
        result.Name.Should().Be(updateInput.Name);
        result.UserId.Should().Be(testUserId);

        notificationContext.HasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCategory_DeletesCategory()
    {
        var dbContext = GetInMemoryDbContext();

        var testUserId = 1;
        var category = new Category { Id = 1, Name = "Test Category", UserId = testUserId };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var mockUserContext = GetMockUserContext(testUserId);
        var notificationContext = new NotificationContext();

        var categoryService = new CategoryService(dbContext, notificationContext, mockUserContext);

        await categoryService.DeleteCategory(category.Id);

        var deletedCategory = await dbContext.Categories.FindAsync(category.Id);

        deletedCategory.Should().BeNull();
    }
}
