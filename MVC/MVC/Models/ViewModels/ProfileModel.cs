using System.ComponentModel.DataAnnotations;

namespace MVC.Models.ViewModels
{
    public class ProfileModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }
}
