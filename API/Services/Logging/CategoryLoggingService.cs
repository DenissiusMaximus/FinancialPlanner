using System;
using API.Dtos;
using API.Inputs;
using API.Services.Category;

namespace API.Services.Logging;

public class CategoryLoggingService(ICategoryService innerService, ILogger<CategoryLoggingService> logger) : ICategoryService
{
    public async Task<CategoryDto?> CreateCategory(CategoryInput input, int userId)
    {
        var result = await innerService.CreateCategory(input, userId);

        if(result != null)
            logger.LogInformation("Category {CategoryId} created successfully for user {UserId}", result.Id, userId);
        else
            logger.LogWarning("Failed to create category {CategoryName} for user {UserId}", input.Name, userId);

        return result;
    }

    public async Task<bool> DeleteCategory(int id, int userId)
    {
        var result = await innerService.DeleteCategory(id, userId);

        if(result)
            logger.LogInformation("Category with ID {CategoryId} deleted successfully for user {UserId}", id, userId);
        else
            logger.LogWarning("Failed to delete category with ID {CategoryId} for user {UserId}", id, userId);

        return result;
    }

    public async Task<List<CategoryDto>> GetCategories(int userId)
    {
        return await innerService.GetCategories(userId);;
    }

    public async Task<CategoryDto?> GetCategoryById(int id, int userId)
    {
        return await innerService.GetCategoryById(id, userId);;
    }

    public async Task<CategoryDto?> UpdateCategory(int id, CategoryInput input, int userId)
    {
        var result = await innerService.UpdateCategory(id, input, userId);

        if(result != null)
            logger.LogInformation("Category with ID {CategoryId} updated successfully for user {UserId}", id, userId);
        else
            logger.LogWarning("Failed to update category with ID {CategoryId} for user {UserId}", id, userId);
            
        return result;
    }
}
