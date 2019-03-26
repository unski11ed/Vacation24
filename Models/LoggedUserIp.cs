using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public class LoggedUserIp
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Ip { get; set; }
        public DateTime Logged { get; set; }
    }
}