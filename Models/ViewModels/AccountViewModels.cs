using System.ComponentModel.DataAnnotations;

namespace EurovisionHub.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Пошта є обов'язковою")]
        [EmailAddress(ErrorMessage = "Некоректний формат пошти")]
        [Display(Name = "Електронна пошта")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Хочу стати Адміністратором контенту")]
        public bool WantsToBeAdmin { get; set; }

        [Display(Name = "Чому ви хочете стати адміном?")]
        [StringLength(500)]
        public string? Motivation { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Пошта є обов'язковою")]
        [EmailAddress(ErrorMessage = "Некоректний формат пошти")]
        [Display(Name = "Електронна пошта")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запам'ятати мене?")]
        public bool RememberMe { get; set; }
    }
}