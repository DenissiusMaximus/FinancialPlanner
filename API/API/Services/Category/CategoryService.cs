using System;
using API.Dtos;
using API.Extensions;
using API.Inputs;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Category;

public class CategoryService(AppDbContext context, NotificationContext notificationContext, ICurrentUserContext currentUserProvider) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryDto>> GetCategories()
    {
        var userId = currentUserProvider.RequiredUserId;
        var rawCategories = context.Categories.Where(c => c.UserId == userId);

        return await rawCategories.AsNoTracking().ProjectToType<CategoryDto>().ToListAsync();
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

        return category.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto?> CreateCategory(CreateCategoryInput input)
    {
        if(string.IsNullOrWhiteSpace(input.Name))
        {
            notificationContext.AddNotification("Name is required", ErrorType.BadRequest);
            return null;
        }

        var userId = currentUserProvider.RequiredUserId;
        var category = input.Adapt<Models.Category>();
        category.UserId = userId;

        var result = context.Categories.Add(category);

        await context.SaveChangesAsync();

        return result.Entity.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto?> UpdateCategory(int id, UpdateCategoryInput input)
    {
        var userId = currentUserProvider.RequiredUserId;
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            notificationContext.AddNotification("Category not found", ErrorType.NotFound);
            return null;
        }

        input.AdaptIgnoreNull(category);

        await context.SaveChangesAsync();

        return category.Adapt<CategoryDto>();
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
