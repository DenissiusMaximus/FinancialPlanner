using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Поле Email є обов'язковим")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле пароль є обов'язковим")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = "User";
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
