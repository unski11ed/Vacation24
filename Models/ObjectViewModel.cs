using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Core;

namespace Vacation24.Models
{
    [Serializable]
    public class ObjectViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public decimal MinimumPrice { get; set; }
        public string Address { get; set; }
        public string Postal { get; set; }
        public string City { get; set; }
        public string Voivoidship { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Web { get; set; }

        public string Type { get; set; }

        public string Region { get; set; }
        public string AdditionalOptions { get; set; }

        public string Lattitude { get; set; }
        public string Longitude { get; set; }

        public string Description { get; set; }
        public string Contact { get; set; }

        public PlaceOptions Options { get; set; }
        public ICollection<Price> Prices { get; set; }

        public static implicit operator ObjectViewModel(Place source)
        {
            return new ObjectViewModel()
            {
                Id = source.Id,
                Name = source.Name,
                MinimumPrice = source.MinimumPrice,
                Address = source.Address,
                Postal = source.Postal,
                City = source.City,
                Voivoidship = source.Voivoidship,
                Phone = source.Phone,
                Email = source.Email,
                Web = source.Web,
                Type = source.Type,
                Region = source.Region,
                AdditionalOptions = source.AdditionalOptions,
                Lattitude = source.Lattitude,
                Longitude = source.Longitude,
                Description = source.Description,
                Contact = source.Contact,
                Options = source.Options,
                Prices = source.Prices
            };
        }
    }

    [Serializable]
    public class RequestById
    {
        public int Id { get; set; }
    }

    public class PromotedObject
    {
        public Place Object {get; set;}
        public bool Promoted { get; set; }
    }
}