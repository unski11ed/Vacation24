using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;
using Vacation24.Models;

namespace Vacation24.Controllers
{
    public class SidebarController : Controller
    {
        private readonly CurrentUserProvider currentUserProvider;

        //
        // GET: /Sidebar/
        public SidebarController(
            CurrentUserProvider currentUserProvider
        )
        {
            this.currentUserProvider = currentUserProvider;
        }

        public ActionResult Right()
        {
            var isLogged = ViewBag.IsLogged = ViewBag.IsOwner = currentUserProvider.IsAuthenticated;

            if (isLogged) {
                ViewBag.IsOwner = currentUserProvider.IsUserInRole("owner");
            }

            return View();
        }

        public ActionResult Left()
        {
            return View();
        }

    }
}
