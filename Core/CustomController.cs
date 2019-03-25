using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Vacation24.Models;
using Vacation24.Core.ExtensionMethods;
using Microsoft.AspNetCore.Identity;

namespace Vacation24.Core
{
    public abstract class CustomController : Controller
    {
        private SignInManager<Profile> signInManager;
        public CustomController(SignInManager<Profile> signInManager)
        {
            checkLockout();

            var newCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;

            this.signInManager = signInManager;
        }

        private async void checkLockout()
        {
            if (User.Identity.IsAuthenticated)
            {
                var context = DefaultContext.GetContext();
                var profile = context.Profiles.Where(
                        p => p.UserId == User.GetUserId()
                    )
                    .FirstOrDefault();
                
                if (profile == null || profile.Locked)
                {
                    await signInManager.SignOutAsync();
                }
            }
        }

        protected bool IsCurrentUserAdmin()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return false;
            }
            return User.IsInRole("admin");
        }
    }
}
