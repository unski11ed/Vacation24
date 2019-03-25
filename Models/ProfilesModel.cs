using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Vacation24.Core;
using Vacation24.Models;

namespace Vacation24.Models
{
    public class Profile: IExtendable<UpdateSellerViewModel>, IExtendable<UpdateUserViewModel>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string NIP { get; set; }

        public bool IsOwner
        {
            get
            {
                return string.IsNullOrEmpty(NIP);
            }
        }

        public string FirstName
        {
            get
            {
                if (IsOwner)
                    return Name;

                var split = Name.Split(' ');
                return split[0];
            }
        }

        public string LastName
        {
            get
            {
                if (IsOwner)
                    return string.Empty;

                var split = Name.Split(' ');
                if (split.Length >= 2)
                {
                    return split[1];
                }

                return string.Empty;
            }
        }

        public string Email
        {
            get
            {
                return UserName;
            }
        }

        public bool Locked { get; set; }

        public virtual PrevilegedContact Contact { get; set; }

        public void Extend(UpdateUserViewModel source)
        {
            throw new NotImplementedException();
        }

        public void Extend(UpdateSellerViewModel source)
        {
            throw new NotImplementedException();
        }
    }

    public class PrevilegedContact
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProfileId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
    }
}