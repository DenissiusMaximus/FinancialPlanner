using System.ComponentModel.DataAnnotations;

namespace MVC.Models.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Поле Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле Пароль є обов'язковим")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль повинен містити щонайменше 8 символів")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Пароль повинен містити цифри, літери та хоча б одну велику літеру")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Оберіть роль")]
        [Display(Name = "Роль")]
        public string Role { get; set; } = "User";
    }
}
