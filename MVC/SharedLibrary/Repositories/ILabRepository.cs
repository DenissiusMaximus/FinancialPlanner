using System.Linq;
using API.Models;

namespace API.Repositories;

public interface ILabRepository
{
    IQueryable<Category> Categories { get; }
    IQueryable<SubCategory> SubCategories { get; }

    void CreateCategory(Category c);
    void SaveCategory(Category c);
    void DeleteCategory(Category c);

    void CreateSubCategory(SubCategory s);
    void SaveSubCategory(SubCategory s);
    void DeleteSubCategory(SubCategory s);
}
