using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Core;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Vacation24.Controllers
{
    public class NewsController : CustomController
    {
        private const int TOTAL_LIST_ENTRIES = 20;
        private readonly DefaultContext dbContext;
        private readonly CurrentUserProvider currentUserProvider;

        public NewsController(
            DefaultContext dbContext,
            CurrentUserProvider currentUserProvider
        )
        {
            this.dbContext = dbContext;
            this.currentUserProvider = currentUserProvider;
        }

        public ActionResult NewsBox(int count = 3)
        {
            ViewBag.News = dbContext.News
                .OrderByDescending(n => n.Created)
                .Take(count)
                .ToList();

            return View();
        }

        [HttpGet]
        public ActionResult Index(int page = 1, string search = null)
        {
            ViewBag.IsEditable = (
                currentUserProvider.IsAuthenticated &&
                currentUserProvider.IsUserInRole("admin")
            );

            if (search != null && search.Length > 3)
                ViewBag.List = dbContext.News
                    .Where(n => n.Title.Contains(search) || n.Content.Contains(search))
                    .OrderByDescending(n => n.Created)
                    .ToPagedList(page, TOTAL_LIST_ENTRIES);
            else
                ViewBag.List = dbContext.News
                    .OrderByDescending(n => n.Created)
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
        [Authorize(Roles = "admin")]
        public ActionResult Create(NewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var profile = dbContext.Profiles.Find(currentUserProvider.UserId);

                News news = model;
                news.Created = news.Updated = DateTime.Now;
                news.ProfileId = profile.Id; ;

                dbContext.News.Add(news);
                dbContext.SaveChanges();

                return Redirect("/News/Index");
            }
            
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult Edit(int id)
        {
            var news = dbContext.News.Find(id);

            if (news == null)
                return NotFound();

            return View(news);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult Edit(NewsViewModel model)
        {
            var dbEntry = dbContext.News.Find(model.Id);

            if (dbEntry == null)
                throw new Exception("Unknown entry requested.");

            dbEntry.Extend(model);

            dbContext.Entry<News>(dbEntry).State = EntityState.Modified;
            dbContext.SaveChanges();

            return View(dbEntry);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult Delete(int id)
        {
            var entry = dbContext.News.Find(id);

            if (entry == null)
                return NotFound();

            dbContext.News.Remove(entry);
            dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult View(int id)
        {
            var entry = dbContext.News.Find(id);
            if (entry == null)
                return NotFound();

            return View(entry);
        }
    }
}
