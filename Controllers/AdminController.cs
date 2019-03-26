using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Payment;
using Vacation24.Models;

namespace Vacation24.Controllers
{
    public class AdminController : Controller
    {
        private readonly IPaymentServices paymentServices;
        private readonly DefaultContext dbContext;

        public AdminController(
            IPaymentServices paymentServices,
            DefaultContext dbContext
        )
        {
            this.paymentServices = paymentServices;
            this.dbContext = dbContext;
        }

        [Authorize(Roles="admin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        public ActionResult ShowUserObjects(string id)
        {
            var profile = dbContext.Profiles.Where(p => p.Id == id).FirstOrDefault();
            if (profile == null)
            {
                return NotFound();
            }

            var count = (decimal)dbContext.Places.Where(p => p.OwnerId == id).Count();   

            ViewBag.Count = Math.Ceiling(count / 4);
            ViewBag.Profile = profile;

            return View();
        }
    }
}
