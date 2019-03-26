using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Vacation24.Models
{
    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Hasło musi mieć min {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Podane hasła nie zgadzają się.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage="Podaj adres email")]
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Niepoprawny adres email.")]
        public string Email { get; set; }

        [Required(ErrorMessage="Podaj hasło")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }
    }

    public interface IRegisterModel
    {
        string UserName { get; set; }
        string Password { get; set; }
        string Address { get; set; }
        bool Tos { get; set; }
    }

    public class RegisterSellerModel : IRegisterModel
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Niepoprawny adres email.")]
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Hasło musi mieć minimum {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Powtórz hasło")]
        [Compare("Password", ErrorMessage = "Podane hasła muszą się zgadzać.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Podaj nazwę firmy")]
        [Display(Name = "Nazwa firmy")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Podaj prawidłowy adres")]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Podaj kod pocztowy")]
        [Display(Name = "Kod Pocztowy")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Podaj miasto")]
        [Display(Name = "Miasto")]
        public string City { get; set; }

        [Required(ErrorMessage="NIP jest wymagany")]
        [Display(Name = "NIP")]
        public string Nip { get; set; }

        //Upoważniony kontakt
        [Display(Name = "Imię")]
        public string ContactFirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string ContactLastName { get; set; }

        [Display(Name = "Telefon")]
        public string ContactPhone { get; set; }
        
        //TOS
        public bool Tos { get; set; }
    }

    public class RegisterUserModel : IRegisterModel
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Niepoprawny adres email")]
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Hasło musi mieć minimum {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Powtórz hasło")]
        [Compare("Password", ErrorMessage = "Podane hasła muszą się zgadzać.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Podaj Imię i nazwisko")]
        [Display(Name = "Imię i Nazwisko")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Podaj prawidłowy adres")]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Podaj kod pocztowy")]
        [Display(Name = "Kod Pocztowy")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Podaj miasto")]
        [Display(Name = "Miasto")]
        public string City { get; set; }

        //TOS
        public bool Tos { get; set; }
    }

    public class PasswordResetRequestModel
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Niepoprawny adres email")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class PasswordResetModel
    {
        [Required]
        public string PasswordResetToken { get; set; }
        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Hasło musi mieć minimum {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło")]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Powtórz nowe hasło")]
        [Compare("NewPassword", ErrorMessage = "Podane hasła muszą się zgadzać.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Aktualne hasło")]
        public string CurrentPassword { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Hasło musi mieć minimum {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło")]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Powtórz nowe hasło")]
        [Compare("NewPassword", ErrorMessage = "Podane hasła muszą się zgadzać.")]
        public string ConfirmPassword { get; set; }
    }
}
