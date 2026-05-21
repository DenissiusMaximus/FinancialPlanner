using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateCategoryInput
    {
        [Required(ErrorMessage = "Назва обов'язкова")]
        public string Name { get; set; } = string.Empty;
    }
}
