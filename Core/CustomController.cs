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
        public CustomController()
        {
            var newCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;
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
