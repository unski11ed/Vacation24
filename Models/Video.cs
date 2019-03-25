using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    //      Json post object
    [Serializable]
    public class SetVideo
    {
        public int postId { get; set; }
        public string url { get; set; }
    }

    //      Database entity
    public class Video
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string EmbedUrl { get; set; }
        public string OriginalUrl { get; set; }
    }
}