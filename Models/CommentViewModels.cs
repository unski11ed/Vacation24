using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Vacation24.Models
{
    public class CommentViewModel
    {
        public int PlaceId { get; set; }
        public string Content { get; set; }

        public static implicit operator Comment(CommentViewModel comment)
        {
            return new Comment()
            {
                PlaceId = comment.PlaceId,
                Content = comment.Content,
            };
        }
    }

    public class MyCommentViewModel
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }
        public string PlaceName { get; set; }

        public string UserId { get; set; }
        public string UserDisplayName { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }

        public static implicit operator MyCommentViewModel(Comment comment)
        {
            return new MyCommentViewModel()
            {
                PlaceId = comment.PlaceId,
                UserId = comment.UserId,
                UserDisplayName = comment.UserDisplayName,
                Content = comment.Content,
                Date = comment.Date
            };
        }
    }

    public class CommentsList
    {
        public List<Comment> Comments { get; set; }
        public int TotalPages { get; set; }
    }
}
