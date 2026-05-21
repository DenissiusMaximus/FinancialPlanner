using System.ComponentModel.DataAnnotations;

namespace MVC.Models.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Введіть Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [UIHint("password")]
        public string Password { get; set; } = string.Empty;
        
        public string? ReturnUrl { get; set; } = "/";
    }
}
