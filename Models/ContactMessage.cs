using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public class ContactMessage
    {
        public int ObjectId { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}