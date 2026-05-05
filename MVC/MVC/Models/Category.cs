using System.ComponentModel.DataAnnotations;

namespace API.Models;

public partial class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a category name")]
    public string Name { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();

    public virtual User User { get; set; } = null!;
}
