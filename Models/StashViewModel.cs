using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    [Serializable]
    public class RequestStashList
    {
        public List<int> Ids { get; set; }
    }

    [Serializable]
    public class RequestStashItem
    {
        public int Id { get; set; }
    }
}