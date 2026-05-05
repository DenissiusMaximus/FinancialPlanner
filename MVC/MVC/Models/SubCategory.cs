using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class SubCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a subcategory name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please specify a category")]
    public int CategoryId { get; set; }

    public virtual Category? Category { get; set; }
}
