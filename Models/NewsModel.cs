using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;

namespace Vacation24.Models
{
    public class News : IExtendable<NewsViewModel>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ProfileId{get;set;}

        public string Title { get; set; }
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
        
        public string ShortContent
        {
            get
            {
                if (Content == null)
                    return null;

                var text = Regex.Replace(Content, "<.*?>", string.Empty);
                var closestSpaceIndex = text.IndexOf(' ', text.Length < 100 ? text.Length - 1 : 100);

                return closestSpaceIndex >= 0 ? text.Substring(0, closestSpaceIndex) + "..." : text;
            }
        }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Profile Profile {get; set;}

        //View model transformation
        public void Extend(NewsViewModel source)
        {
            Title = source.Title;
            Content = source.Content;
        }

        public static implicit operator News(NewsViewModel model)
        {
            var output = new News();
            output.Extend(model);
            return output;
        }
    }

    public class NewsViewModel{
        [HiddenInput]
        public int Id { get; set; }

        [Display(Name = "Tytuł")]
        [DataType(DataType.Text)]
        [StringLength(200, ErrorMessage = "Pole musi zawierać minimum {2} znków", MinimumLength = 5)]
        public string Title { get; set; }

        [Display(Name = "Treść")]
        [DataType(DataType.MultilineText)]
        [StringLength(5000, ErrorMessage = "Pole musi zawierać minimum {2} znków", MinimumLength = 50)]
        public string Content { get; set; }
    }
}