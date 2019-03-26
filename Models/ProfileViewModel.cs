using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public class UpdateUserViewModel
    {
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

        public static implicit operator UpdateUserViewModel(Profile profile)
        {
            return new UpdateUserViewModel()
            {
                Name = profile.Name,
                Address = profile.Address,
                PostalCode = profile.PostalCode
            };
        }
    }

    public class UpdateSellerViewModel
    {
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

        [Required(ErrorMessage = "NIP jest wymagany")]
        [Display(Name = "NIP")]
        public string Nip { get; set; }

        //Upoważniony kontakt
        [Display(Name = "Imię")]
        public string ContactFirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string ContactLastName { get; set; }

        [Display(Name = "Telefon")]
        public string ContactPhone { get; set; }

        public static implicit operator UpdateSellerViewModel(Profile profile)
        {
            return new UpdateSellerViewModel()
            {
                CompanyName = profile.Name,
                Address = profile.Address,
                PostalCode = profile.PostalCode,
                City = profile.City,
                Nip = profile.NIP,
                ContactFirstName = profile.Contact.FirstName,
                ContactLastName = profile.Contact.LastName,
                ContactPhone = profile.Contact.Phone
            };
        }
    }

    public class ProfileDetails
    {
        private string _email;

        public string UserId {get;set;}

        public string UserName { 
            set {
                _email = value;
            } 
        }

        public string Email {
            get
            {
                return _email;
            }
        }
        public string Name{get;set;}
        
        public string Address{get;set;}
        public string PostalCode {get;set;}
        public string City {get;set;}

        public string Nip {get;set;}

        public PrevilegedContact Contact {get;set;}

        public bool SubscriptionEnabled {get;set;}
        public DateTime SubscriptionExpiriation{get;set;}
        public int SubscriptionId { get; set; }

        public bool IsActive {get;set;}

        public bool Locked { get; set; }

        public bool IsOwner
        {
            get
            {
                if (Roles != null)
                {
                    return Roles.Contains("owner");
                }
                return false;
            }
        }

        public string[] Roles { get; set; }

        public static implicit operator ProfileDetails(Profile profile)
        {
            return new ProfileDetails()
            {
                UserId = profile.Id,
                UserName = profile.UserName,
                Name = profile.Name,
                Address = profile.Address,
                PostalCode = profile.PostalCode,
                City = profile.City,
                Nip = profile.NIP,
                Contact = profile.Contact,
                Locked = profile.Locked
            };
        }
    }
}