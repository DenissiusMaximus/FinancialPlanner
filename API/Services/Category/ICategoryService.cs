using API.Dtos;
using API.Inputs;

namespace API.Services.Category;

public interface ICategoryService
{
    Task<CategoryDto?> CreateCategory(CategoryInput input, int userId);
    Task<bool> DeleteCategory(int id, int userId);
    Task<List<CategoryDto>> GetCategories(int userId);
    Task<CategoryDto?> GetCategoryById(int id, int userId);
    Task<CategoryDto?> UpdateCategory(int id, CategoryInput input, int userId);
}
