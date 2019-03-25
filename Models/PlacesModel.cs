using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Vacation24.Core;
using Vacation24.Models;

namespace Vacation24.Models
{
    public class Place : IExtendable<ObjectViewModel>
    {
        public Place()
        {
            Region = AdditionalOptions = string.Empty;
            Created = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OwnerId { get; set; }

        public string Name { get; set; }
        public decimal MinimumPrice { get; set; }
        public string Address { get; set; }
        public string Postal { get; set; }
        public string City { get; set; }
        public string Voivoidship { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Web { get; set; }

        public string Region { get; set; }
        public string AdditionalOptions { get; set; }

        public string Type { get; set; }

        public string Lattitude { get; set; }
        public string Longitude { get; set; }

        public string Description { get; set; }
        public string Contact { get; set; }

        public DateTime Created { get; set; }

        [DefaultValue(true)]
        public bool IsPaid { get; set; }

        [DefaultValue(0)]
        public int Views { get; set; }

        [DefaultValue(0)]
        public int UniqueViews { get; set; }
        
        public string ShortDescription
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                    return Description;

                var text = Regex.Replace(Description, "<.*?>", string.Empty);
                var closestSpaceIndex = text.IndexOf(' ', text.Length < 100 ? text.Length - 1 : 100);

                return closestSpaceIndex >= 0 ? text.Substring(0, closestSpaceIndex) + "..." : text;
            }
        }

        public Photo MainPhoto
        {
            get
            {
                return Photos.Where(p => p.Type == PhotoType.Main).FirstOrDefault();
            }
        }

        public virtual ICollection<Price> Prices { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public virtual PlaceOptions Options { get; set; }


        //View model handling
        public static implicit operator Place(ObjectViewModel model)
        {
            var place = new Place();
            place.Extend(model);
            return place;
        }

        public void Extend(ObjectViewModel source)
        {
            this.AdditionalOptions = source.AdditionalOptions;
            this.Address = source.Address;
            this.City = source.City;
            this.Contact = source.Contact;
            this.Description = source.Description;
            this.Email = source.Email;
            this.Lattitude = source.Lattitude;
            this.Longitude = source.Longitude;
            this.MinimumPrice = source.MinimumPrice;
            this.Name = source.Name;
            this.Phone = source.Phone;
            this.Region = source.Region;
            this.Type = source.Type;
            this.Voivoidship = source.Voivoidship;
            this.Web = source.Web;
            this.Postal = source.Postal;

            this.Prices = source.Prices != null ? source.Prices : new List<Price>();
            this.Options = source.Options;
        }
    }

    public class Price
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PlaceId { get; set; }

        public string Name { get; set; }
        public string Duration { get; set; }
        public decimal Value { get; set; }
    }

    public enum PhotoType{
        Main,
        Additional
    }

    public class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PlaceId { get; set; }

        public string Filename { get; set; }
        public PhotoType Type { get; set; } 
    }

    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PlaceId { get; set; }

        public int UserId { get; set; }
        public string UserDisplayName { get; set; }

        public string Content { get; set; }

        public string Ip { get; set; }

        public DateTime Date { get; set; }
    }

    public class PlaceOptions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PlaceId { get; set; }

        public bool Barbeque { get; set; }
        public bool Shower { get; set; }
        public bool Tv { get; set; }
        public bool Internet { get; set; }
        public bool Kitchen { get; set; }
        public bool Parking { get; set; }
        public bool Food { get; set; }
        public bool Bicycles { get; set; }
        public bool Disableds { get; set; }
        public bool Pets { get; set; }
        public bool Radio { get; set; }
        public bool Playground { get; set; }
        public bool PaymentCards { get; set; }
        public bool Kettle { get; set; }
        public bool Garden { get; set; }
        public bool Fenced { get; set; }
        public bool Sauna { get; set; }
        public bool Pool { get; set; }
        public bool AirConditioning { get; set; }
        public bool Fitness { get; set; }
        public bool Ski { get; set; }
        public bool ConnferenceRoom { get; set; }
    }
}