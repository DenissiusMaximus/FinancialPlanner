using System;
using API.Dtos;
using API.Inputs;
using API.Utils.Notification;
using API.Utils.UserContext;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Category;

public class CategoryService(AppDbContext context, NotificationContext notificationContext, ICurrentUserProvider currentUserProvider) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryDto>> GetCategories()
    {
        var userId = currentUserProvider.RequiredUserId;
        var rawCategories = context.Categories.Where(c => c.UserId == userId);

        return await rawCategories.AsNoTracking().Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            UserId = c.UserId
        }).ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryById(int id)
    {
        var userId = currentUserProvider.RequiredUserId;
        var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            notificationContext.AddNotification("Category not found", ErrorType.NotFound);
            return null;
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            UserId = category.UserId
        };
    }

    public async Task<CategoryDto?> CreateCategory(CategoryInput input)
    {
        var userId = currentUserProvider.RequiredUserId;
        var category = new Models.Category
        {
            Name = input.Name,
            UserId = userId
        };

        var result = context.Categories.Add(category);

        await context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = result.Entity.Id,
            Name = result.Entity.Name,
            UserId = result.Entity.UserId
        };
    }

    public async Task<CategoryDto?> UpdateCategory(int id, CategoryInput input)
    {
        var userId = currentUserProvider.RequiredUserId;
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            notificationContext.AddNotification("Category not found", ErrorType.NotFound);
            return null;
        }

        category.Name = input.Name;

        await context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            UserId = category.UserId
        };
    }

    public async Task<bool> DeleteCategory(int id)
    {
        var userId = currentUserProvider.RequiredUserId;
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            notificationContext.AddNotification("Category not found", ErrorType.NotFound);
            return false;
        }

        context.Categories.Remove(category);

        return await context.SaveChangesAsync() > 0;
    }
}
