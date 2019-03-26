using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Vacation24.Models;

namespace Vacation24.Controllers
{
    public class CommentsController : CustomController
    {
        private readonly AppConfiguration configuration;
        private readonly DefaultContext dbContext;
        private readonly CurrentUserProvider currentUserProvider;

        private int commentsPerPage => configuration.SiteConfiguration.CommentsPerPage;

        public CommentsController(
            AppConfiguration configuration,
            DefaultContext dbContext,
            CurrentUserProvider currentUserProvider
        ) {
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.currentUserProvider = currentUserProvider;
        }

        public ActionResult Get(int objectId, int? page)
        {
            page = page == null ? 0 : page;

            decimal totalComments = dbContext.Comments
                .Where(c => c.PlaceId == objectId)
                .Count();

            var comments = dbContext.Comments
                .Where(c => c.PlaceId == objectId)
                .OrderByDescending(c => c.Date)
                .Skip((int)(page * commentsPerPage))
                .Take(commentsPerPage)
                .ToList();

            var commentsList = new CommentsList()
            {
                Comments = comments,
                TotalPages = (int)Math.Ceiling(totalComments / commentsPerPage)
            };

            return Json(commentsList);
        }

        [Authorize(Roles = "user, owner, admin")]
        public ActionResult ShowMyComments(int? page)
        {
            page = page == null ? 0 : page;

            decimal totalComments = dbContext.Comments
                .Where(c => c.UserId == currentUserProvider.UserId)
                .Count();

            var comments = dbContext.Comments
                .Where(c => c.UserId == currentUserProvider.UserId)
                .OrderByDescending(c => c.Date)
                .ThenBy(c => c.PlaceId)
                .Skip((int)(page * commentsPerPage))
                .Take(commentsPerPage)
                .ToList()
                .Select((comment) => {
                    MyCommentViewModel commentVm = comment;
                    commentVm.PlaceName = dbContext.Places.Find(comment.PlaceId).Name;
                    return commentVm;
                });

            ViewBag.Comments = comments;
            ViewBag.Count = Math.Ceiling(totalComments / commentsPerPage);

            return View();
        }

        [AllowAnonymous]
        public ActionResult Add(CommentViewModel comment)
        {
            var commentModel = (Comment)comment;
            var user = dbContext.Profiles
                .Find(currentUserProvider.UserId);

            var isLogged = user != null;
            var userIp = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            var userId = user != null ? user.Id : string.Empty;
            var alreadyCommented = dbContext.Comments.Any(
                c => (
                    EF.Functions.DateDiffMinute(c.Date, DateTime.Now) < 5 &&
                    (!isLogged ? c.Ip == userIp : c.UserId == userId)
                )
            );

            if (alreadyCommented) {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "You have added a comment recently. Try again later."
                });
            }

            commentModel.Ip = userIp;
            commentModel.Date = DateTime.Now;
            commentModel.UserId = user != null ? currentUserProvider.UserId : String.Empty;
            commentModel.UserDisplayName = user != null ? user.Name : "Anonymous";

            dbContext.Comments.Add(commentModel);

            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception)
            {
                return Json(new ResultViewModel() { Status = (int)ResultStatus.Error, Message = "Failed to append comment" });
            }

            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }

        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Delete(int id)
        {
            var comment = dbContext.Comments.FirstOrDefault(c => c.Id == id);

            //Check if comment owner or if admin
            if (comment != null)
            {
                if (
                    //User is not admin (user, owner) AND
                    !currentUserProvider.IsUserInRole("admin") &&
                    //Comment owner Id != Logged User Id
                    comment.UserId != currentUserProvider.UserId
                )
                {
                    //Return access denied
                    return Json(new ResultViewModel()
                    {
                        Status = (int) ResultStatus.Error,
                        Message = "Access denied"
                    });
                }

                dbContext.Comments.Remove(comment);
            }
                
            return Json(new ResultViewModel() { Status = (int) ResultStatus.Success });
        }
    }
}
