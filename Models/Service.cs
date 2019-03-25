using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public class Service
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string HandlerName { get; set; }

        public decimal Price { get; set; }

        public int Days { get; set; }
    }

    public interface IActiveService
    {
        int Id { get; set; }

        string HandlerName { get; set; }

        DateTime ExpiriationTime { get; set; }
    }
}