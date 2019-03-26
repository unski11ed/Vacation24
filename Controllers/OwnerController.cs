using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Core;
using Vacation24.Core.Payment;
using Vacation24.Services;
using Microsoft.AspNetCore.Authorization;

namespace Vacation24.Controllers
{
    public class OwnerController : CustomController
    {
        //
        // GET: /Owner/
        private const int MAX_OBJECTS_PER_PAGE = 4;

        private readonly IPaymentServices paymentServices;
        private readonly DefaultContext dbContext;
        private readonly CurrentUserProvider currentUserProvider;

        public OwnerController(
            IPaymentServices paymentServices,
            DefaultContext dbContext,
            CurrentUserProvider currentUserProvider
        )
        {
            this.paymentServices = paymentServices;
            this.dbContext = dbContext;
            this.currentUserProvider = currentUserProvider;
        }

        [HttpGet]
        [Authorize(Roles = "owner, admin")]
        public ActionResult Index()
        {
            var totalUserObjects = dbContext.Places
                .Where(p => p.OwnerId == currentUserProvider.UserId)
                .Count();

            ViewBag.TotalPages = Math.Ceiling((decimal)totalUserObjects / MAX_OBJECTS_PER_PAGE);

            ViewBag.TotalUniqueViews = totalUserObjects >= 1 ?
                dbContext.Places
                    .Where(p => p.OwnerId == currentUserProvider.UserId)
                    .Select(p => p.UniqueViews)
                    .Sum() :
                0;

            ViewBag.TotalViews = totalUserObjects >= 1 ?
                dbContext.Places
                    .Where(p => p.OwnerId == currentUserProvider.UserId)
                    .Select(p => p.Views)
                    .Sum() :
                0;

            var subscription = paymentServices
                .GetUserServices<SubscriptionService>(currentUserProvider.UserId)
                .FirstOrDefault();

            ViewBag.IsSubscribed = IsCurrentUserAdmin() || (subscription != null && subscription.IsActive);

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "owner, admin")]
        public ActionResult GetUserObjects(int page = 0, string userId = "")
        {
            var uId = IsCurrentUserAdmin() && userId != String.Empty ? userId : currentUserProvider.UserId;

            var objects = dbContext.Places
                .Where(p => p.OwnerId == uId)
                .OrderByDescending(p => p.Created)
                .Skip(MAX_OBJECTS_PER_PAGE * page)
                .Take(MAX_OBJECTS_PER_PAGE)
                .ToList();

            return View(objects);
        }

    }
}
