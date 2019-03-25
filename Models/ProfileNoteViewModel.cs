using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public class ProfileNoteViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
    }
}