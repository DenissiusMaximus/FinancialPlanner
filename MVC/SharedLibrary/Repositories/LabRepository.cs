using System.Linq;
using API.Models;

namespace API.Repositories;

public class LabRepository : ILabRepository
{
    private readonly AppDbContext _context;

    public LabRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Category> Categories => _context.Categories;
    public IQueryable<SubCategory> SubCategories => _context.SubCategories;

    public void CreateCategory(Category c)
    {
        _context.Categories.Add(c);
        _context.SaveChanges();
    }

    public void SaveCategory(Category c)
    {
        _context.SaveChanges();
    }

    public void DeleteCategory(Category c)
    {
        _context.Categories.Remove(c);
        _context.SaveChanges();
    }

    public void CreateSubCategory(SubCategory s)
    {
        _context.SubCategories.Add(s);
        _context.SaveChanges();
    }

    public void SaveSubCategory(SubCategory s)
    {
        _context.SaveChanges();
    }

    public void DeleteSubCategory(SubCategory s)
    {
        _context.SubCategories.Remove(s);
        _context.SaveChanges();
    }
}
