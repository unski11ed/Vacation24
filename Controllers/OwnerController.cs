using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using WebMatrix.WebData;
using PagedList;
using PagedList.Mvc;
using Vacation24.Core;
using Vacation24.Core.Payment;
using Vacation24.Services;

namespace Vacation24.Controllers
{
    public class OwnerController : CustomController
    {
        //
        // GET: /Owner/
        private const int MAX_OBJECTS_PER_PAGE = 4;

        private DefaultContext _dbContext = new DefaultContext();
        private IPaymentServices _paymentServices;

        public OwnerController(IPaymentServices paymentServices)
        {
            _paymentServices = paymentServices;
        }

        [HttpGet]
        [Authorize(Roles = "owner, admin")]
        public ActionResult Index()
        {
            var totalUserObjects = _dbContext.Places.Where(p => p.OwnerId == WebSecurity.CurrentUserId)
                                                    .Count();

            ViewBag.TotalPages = Math.Ceiling((decimal)totalUserObjects / MAX_OBJECTS_PER_PAGE);

            ViewBag.TotalUniqueViews = totalUserObjects >= 1 ?
                                       _dbContext.Places.Where(p => p.OwnerId == WebSecurity.CurrentUserId)
                                                        .Select(p => p.UniqueViews).Sum()
                                                        : 0;

            ViewBag.TotalViews = totalUserObjects >= 1 ?
                                       _dbContext.Places.Where(p => p.OwnerId == WebSecurity.CurrentUserId)
                                                        .Select(p => p.Views).Sum()
                                                        : 0;

            var subscription = _paymentServices.GetUserServices<SubscriptionService>(WebSecurity.CurrentUserId)
                                               .FirstOrDefault();

            ViewBag.IsSubscribed = IsCurrentUserAdmin() || (subscription != null && subscription.IsActive);


            return View();
        }

        [HttpGet]
        [Authorize(Roles = "owner, admin")]
        public ActionResult GetUserObjects(int page = 0, int userId = -1)
        {
            var uid = IsCurrentUserAdmin() && userId > 0 ? userId : WebSecurity.CurrentUserId;

            var objects = _dbContext.Places.Where(p => p.OwnerId == uid)
                                           .OrderByDescending(p => p.Created)
                                           .Skip(MAX_OBJECTS_PER_PAGE * page)
                                           .Take(MAX_OBJECTS_PER_PAGE)
                                           .ToList();

            return View(objects);
        }

    }
}
