using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Payment;
using Vacation24.Models;
using Vacation24.Models.DTO;

namespace Vacation24.Controllers
{
    public class AdminController : Controller
    {
        private IPaymentServices _services;
        private DefaultContext _context = DefaultContext.GetContext();

        public AdminController(IPaymentServices services)
        {
            _services = services;
        }

        [Authorize(Roles="admin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        public ActionResult ShowUserObjects(int id)
        {
            var profile = _context.Profiles.Where(p => p.UserId == id).FirstOrDefault();
            if (profile == null)
            {
                return HttpNotFound();
            }

            var count = (decimal)_context.Places.Where(p => p.OwnerId == id).Count();   

            ViewBag.Count = Math.Ceiling(count / 4);
            ViewBag.Profile = profile;

            return View();
        }
    }
}
