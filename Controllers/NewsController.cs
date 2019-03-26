using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Web.Security;
using Vacation24.Models;
using WebMatrix.WebData;
using PagedList;
using PagedList.Mvc;
using Vacation24.Core;

namespace Vacation24.Controllers
{
    public class NewsController : CustomController
    {
        private DefaultContext _dbContext = new DefaultContext();
        private const int TOTAL_LIST_ENTRIES = 20;

        public ActionResult NewsBox(int count = 3)
        {
            //TODO: Write
            ViewBag.News = _dbContext.News.OrderByDescending(n => n.Created)
                                          .Take(count)
                                          .ToList();

            return View();
        }

        [HttpGet]
        public ActionResult Index(int page = 1, string search = null)
        {
            ViewBag.IsEditable = WebSecurity.IsAuthenticated && Roles.IsUserInRole("admin");

            if (search != null && search.Length > 3)
                ViewBag.List = _dbContext.News.Where(n => n.Title.Contains(search) || n.Content.Contains(search))
                                              .OrderByDescending(n => n.Created)
                                              .ToPagedList(page, TOTAL_LIST_ENTRIES);
            else
                ViewBag.List = _dbContext.News.OrderByDescending(n => n.Created)
                                              .ToPagedList(page, TOTAL_LIST_ENTRIES);

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult Create()
        {
            return View();
        }
    
        [HttpPost]
        [ValidateInput(false)]
        [Authorize(Roles = "admin")]
        public ActionResult Create(NewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var profile = _dbContext.Profiles.Where(p => p.UserId == WebSecurity.CurrentUserId).First();

                var news = (News)model;
                news.Created = news.Updated = DateTime.Now;
                news.ProfileId = profile.Id; ;

                _dbContext.News.Add(news);

                _dbContext.SaveChanges();

                return Redirect("/News/Index");
            }
            
            return View();
        }

        [HttpGet]
        [ValidateInput(false)]
        [Authorize(Roles = "admin")]
        public ActionResult Edit(int id)
        {
            var news = _dbContext.News.Find(id);

            if (news == null)
                return HttpNotFound();

            return View(news);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize(Roles = "admin")]
        public ActionResult Edit(NewsViewModel model)
        {
            var dbEntry = _dbContext.News.Find(model.Id);

            if (dbEntry == null)
                throw new Exception("Unknown entry requested.");

            dbEntry.Extend(model);

            _dbContext.Entry<News>(dbEntry).State = System.Data.Entity.EntityState.Modified;
            _dbContext.SaveChanges();

            return View(dbEntry);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult Delete(int id)
        {
            var entry = _dbContext.News.Find(id);

            if (entry == null)
                return HttpNotFound();

            _dbContext.News.Remove(entry);
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult View(int id)
        {
            var entry = _dbContext.News.Find(id);
            if (entry == null)
                return HttpNotFound();

            return View(entry);
        }
    }
}
