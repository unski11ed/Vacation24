using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Web.Security;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Vacation24.Models;
using Vacation24.Models.DTO;
using WebMatrix.WebData;

namespace Vacation24.Controllers
{
    public class CommentsController : CustomController
    {
        private DefaultContext _dbContext = new DefaultContext();

        private int commentsPerPage
        {
            get
            {
                return AppConfiguration.Get().SiteConfiguration.CommentsPerPage;
            }
        }


        public ActionResult Get(int objectId, int? page)
        {
            page = page == null ? 0 : page;

            decimal totalComments = _dbContext.Comments.Where(c => c.PlaceId == objectId)
                                                   .Count();

            var comments = _dbContext.Comments.Where(c => c.PlaceId == objectId)
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

            decimal totalComments = _dbContext.Comments.Where(c => c.UserId == WebSecurity.CurrentUserId)
                                                       .Count();

            var output = new List<MyCommentViewModel>();

            var comments = _dbContext.Comments.Where(c => c.UserId == WebSecurity.CurrentUserId)
                                              .OrderByDescending(c => c.Date)
                                              .ThenBy(c => c.PlaceId)
                                              .Skip((int)(page * commentsPerPage))
                                              .Take(commentsPerPage)
                                              .ToList();
            //HAXXXXXXXXXXXXXXXXXXXXXX
            foreach (var comment in comments)
            {
                var c = (MyCommentViewModel)comment;
                c.PlaceName = _dbContext.Places.Find(comment.PlaceId).Name;
                output.Add(c);
            }

            ViewBag.Comments = output;
            ViewBag.Count = Math.Ceiling(totalComments / commentsPerPage);

            return View();
        }

        //[Authorize(Roles = "user, owner, admin")]
        [AllowAnonymous]
        public ActionResult Add(CommentViewModel comment)
        {
            var commentModel = (Comment)comment;
            var user = _dbContext.Profiles.Where(p => p.UserId == WebSecurity.CurrentUserId).FirstOrDefault();

            var isLogged = user != null;
            var userIp = Request.UserHostAddress;
            var userId = user != null ? user.Id : -1;

            if (_dbContext.Comments.Any(
                    c => DbFunctions.DiffMinutes(c.Date, DateTime.Now) < 5 &&   //Check comments added less then the time provided
                    (!isLogged ? c.Ip == userIp : c.UserId == userId)))                            //If anonymous - check by IP address
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Dopiero dodałeś komentarz. Spróbuj za jakiś czas."
                });
            }

            commentModel.Ip = Request.UserHostAddress;
            commentModel.Date = DateTime.Now;
            commentModel.UserId = user != null ? WebSecurity.CurrentUserId : -1;
            commentModel.UserDisplayName = user != null ? user.Name : "Anonim";

            _dbContext.Comments.Add(commentModel);

            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception)
            {
                return Json(new ResultViewModel() { Status = (int)ResultStatus.Error, Message = "Nie udało się zapisać komentarza" });
            }

            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }

        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Delete(int id)
        {
            var comment = _dbContext.Comments.FirstOrDefault(c => c.Id == id);

            //Check if comment owner or if admin
            if (comment != null)
            {
                if(!Roles.GetRolesForUser().Contains("admin") && //User is not admin (user, owner) AND
                    comment.UserId != WebSecurity.CurrentUserId) //Comment owner Id != Logged User Id
                {
                    //Return access denied
                    return Json(new ResultViewModel()
                    {
                        Status = (int) ResultStatus.Error,
                        Message = "Dostęp zabroniony"
                    });
                }

                _dbContext.Comments.Remove(comment);
            }
                
            return Json(new ResultViewModel() { Status = (int) ResultStatus.Success });
        }
    }
}
