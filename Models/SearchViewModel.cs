using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    [Serializable]
    public class RequestCities
    {
        public string Name { get; set; }
    }

    [Serializable]
    public class QueryFilter
    {
        public string Name { get; set; }
        public string UrlParam { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
    }

    [Serializable]
    public class RequestPopularCitites
    {
        public string Criteria { get; set; }
    }

    [Serializable]
    public class CititesInVoivoidshipWithCount
    {
        public string City { get; set; }
        public int PlacesCount { get; set; }
    }

}