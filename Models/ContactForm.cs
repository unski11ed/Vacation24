using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public class ContactFormInitData
    {
        public string Subject { get; set; }
    }

    public class ContactFormMessage
    {
        [Required(ErrorMessage="Podaj swój adres email")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Niepoprawny adres email")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        [Required(ErrorMessage="Temat wiadomości jest wymagany")]
        [Display(Name = "Temat")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Treść wiadomości jest wymagana")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Treść")]
        public string Content { get; set; }
    }
}