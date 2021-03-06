﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public enum SpecialOfferPlacement
    {
        HomePage,
        SideBar,
        SearchResults
    }

    public class SpecialOffer : IActiveService
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }

        public SpecialOfferPlacement Placement { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime ExpiriationTime { get; set; }

        public string HandlerName { get; set; }

        public bool IsActive
        {
            get
            {
                return ExpiriationTime >= DateTime.Now;
            }
        }

        public virtual Place Place { get; set; }
    }
}