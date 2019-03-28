using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Core.Payment;
using Vacation24.Services;
using Vacation24.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using X.PagedList;

namespace Vacation24.Controllers
{
    public class ProfileController : CustomController
    {
        private const int USERS_PER_PAGE = 30;
        private readonly IPaymentServices paymentServices;
        private readonly DefaultContext dbContext;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly UserManager<Profile> userManager;

        //
        // GET: /Profile/

        public ProfileController(
            IPaymentServices paymentServices,
            DefaultContext dbContext,
            ICurrentUserProvider currentUserProvider,
            UserManager<Profile> userManager
        )
        {
            this.paymentServices = paymentServices;
            this.dbContext = dbContext;
            this.currentUserProvider = currentUserProvider;
            this.userManager = userManager;
        }

        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Index()
        {
            ViewBag.IsAdmin = (
                currentUserProvider.IsUserInRole("admin") ||
                currentUserProvider.IsUserInRole("owner")
            );

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Edit()
        {
            var profile = dbContext.Profiles
                .Where(p => p.Id == currentUserProvider.UserId)
                .FirstOrDefault();

            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.isSeller = currentUserProvider.IsUserInRole("owner");
            
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
                var profile = dbContext.Profiles
                    .Where(p => p.Id == currentUserProvider.UserId)
                    .FirstOrDefault();

                if (profile == null)
                {
                    return NotFound();
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

                dbContext.Entry<Vacation24.Models.Profile>(profile).State = EntityState.Modified;
                dbContext.SaveChanges();

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
                var profile = dbContext.Profiles
                    .Where(p => p.Id == currentUserProvider.UserId)
                    .FirstOrDefault();

                if (profile == null)
                {
                    return NotFound();
                }

                profile.Extend(model);

                dbContext.Entry<Vacation24.Models.Profile>(profile).State = EntityState.Modified;
                dbContext.SaveChanges();

                return RedirectToAction("Edit", "Profile");
            }
            return View("Edit", model);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> List(string sort = "Id", string sortdir = "ASC", string type = "all", int page = 1)
        {
            //Hacky
            IQueryable<Models.Profile> profiles;

            switch(type){
                case "all":
                default:
                    profiles = dbContext.Profiles;
                break;

                case "user":
                    profiles = dbContext.Profiles.Where(p => p.NIP == "");
                break;

                case "owner":
                    profiles = dbContext.Profiles.Where(p => p.NIP != "");
                break;
            }

            var profilesList = profiles
                .OrderBy(String.Format("{0} {1}", sort, sortdir))
                .Skip((page - 1) * USERS_PER_PAGE)
                .Take(USERS_PER_PAGE)
                .ToList();

            var output = new List<ProfileDetails>();

            foreach (var profile in profilesList)
            {
                //Fill standard profile data
                var outputItem = (ProfileDetails)profile;

                outputItem.IsActive = profile.EmailConfirmed;

                outputItem.Roles = (await userManager.GetRolesAsync(profile)).ToArray();

                if (outputItem.IsOwner)
                {
                    //Get subscription state
                    var subscriptionService = paymentServices
                        .GetUserServices<SubscriptionService>(outputItem.UserId)
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
        public ActionResult LockUser(string userId)
        {
            var profile = dbContext.Profiles
                .Where(p => p.Id == userId)
                .FirstOrDefault();

            if (profile == null)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znaleziono użytkownika"
                });
            }

            profile.Locked = !profile.Locked;
            dbContext.Entry<Vacation24.Models.Profile>(profile).State = EntityState.Modified;
            dbContext.SaveChanges();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = profile.Locked ? "Zablokowany" : "Aktywny"
            });
        }
    }
}
