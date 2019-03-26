using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Web.Security;
using Vacation24.Models;
using Vacation24.Models.DTO;
using WebMatrix.WebData;
using PagedList;
using PagedList.Mvc;
using Vacation24.Core.Payment;
using Vacation24.Services;
using Vacation24.Core;
using Linq.Dynamic;

namespace Vacation24.Controllers
{
    public class ProfileController : CustomController
    {
        private const int USERS_PER_PAGE = 30;

        //
        // GET: /Profile/
        DefaultContext _dbContext = new DefaultContext();
        private IPaymentServices _paymentServices;

        public ProfileController(IPaymentServices paymentServices)
        {
            _paymentServices = paymentServices;
        }

        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Index()
        {
            ViewBag.IsAdmin = Roles.IsUserInRole("admin") || Roles.IsUserInRole("owner");

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Edit()
        {
            var profile = _dbContext.Profiles.Where(p => p.UserId == WebSecurity.CurrentUserId).FirstOrDefault();

            if (profile == null)
            {
                return HttpNotFound();
            }

            ViewBag.isSeller = Roles.IsUserInRole("owner");
            
            object viewModel;
            if (ViewBag.isSeller)
                viewModel = (UpdateSellerViewModel)profile;
            else
                viewModel = (UpdateUserViewModel)profile;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "owner, admin")]
        public ActionResult EditSeller(UpdateSellerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var profile = _dbContext.Profiles.Where(p => p.UserId == WebSecurity.CurrentUserId).FirstOrDefault();

                if (profile == null)
                {
                    return HttpNotFound();
                }

                profile.Extend(model);
                if (profile.Contact == null)
                {
                    profile.Contact = new PrevilegedContact()
                    {
                        FirstName = model.ContactFirstName,
                        LastName = model.ContactLastName,
                        Phone = model.ContactPhone
                    };
                }

                _dbContext.Entry<Vacation24.Models.Profile>(profile).State = System.Data.Entity.EntityState.Modified;
                _dbContext.SaveChanges();

                return RedirectToAction("Edit", "Profile");
            }

            return View("Edit", model);
        }

        [HttpPost]
        [Authorize(Roles = "user, admin")]
        public ActionResult EditUser(UpdateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var profile = _dbContext.Profiles.Where(p => p.Id == WebSecurity.CurrentUserId).FirstOrDefault();

                if (profile == null)
                {
                    return HttpNotFound();
                }

                profile.Extend(model);

                _dbContext.Entry<Vacation24.Models.Profile>(profile).State = System.Data.Entity.EntityState.Modified;
                _dbContext.SaveChanges();

                return RedirectToAction("Edit", "Profile");
            }
            return View("Edit", model);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult List(string sort = "Id", string sortdir = "ASC", string type = "all", int page = 1)
        {
            //Hacky
            IQueryable<Models.Profile> profiles;

            switch(type){
                case "all":
                default:
                    profiles = _dbContext.Profiles;
                break;

                case "user":
                    profiles = _dbContext.Profiles.Where(p => p.NIP == "");
                break;

                case "owner":
                    profiles = _dbContext.Profiles.Where(p => p.NIP != "");
                break;
            }

            var profilesList = profiles.OrderBy(string.Format("{0} {1}", sort, sortdir))
                                        .Skip((page - 1) * USERS_PER_PAGE)
                                        .Take(USERS_PER_PAGE)
                                        .ToList();

            var output = new List<ProfileDetails>();

            foreach (var profile in profilesList)
            {
                //Fill standard profile data
                var outputItem = (ProfileDetails)profile;

                outputItem.IsActive = WebSecurity.IsConfirmed(outputItem.Email);

                outputItem.Roles = Roles.GetRolesForUser(outputItem.Email);

                if (outputItem.IsOwner)
                {
                    //Get subscription state
                    var subscriptionService = _paymentServices.GetUserServices<SubscriptionService>(outputItem.UserId)
                                                              .FirstOrDefault();
                    //Pass subscription state
                    outputItem.SubscriptionEnabled = subscriptionService != null && subscriptionService.IsActive;
                    if (outputItem.SubscriptionEnabled)
                    {
                        outputItem.SubscriptionExpiriation = subscriptionService.Expiriation;
                        outputItem.SubscriptionId = subscriptionService.Id;
                    }
                        
                }

                output.Add(outputItem);
            }

            var pagedList = new StaticPagedList<ProfileDetails>(output, page, USERS_PER_PAGE, profilesList.Count());

            return View(pagedList);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult LockUser(int userId)
        {
            var profile = _dbContext.Profiles.Where(p => p.UserId == userId).FirstOrDefault();

            if (profile == null)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znaleziono użytkownika"
                }, JsonRequestBehavior.AllowGet);
            }

            profile.Locked = !profile.Locked;
            _dbContext.Entry<Vacation24.Models.Profile>(profile).State = System.Data.Entity.EntityState.Modified;
            _dbContext.SaveChanges();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = profile.Locked ? "Zablokowany" : "Aktywny"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
