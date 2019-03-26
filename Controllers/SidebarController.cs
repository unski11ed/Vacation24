using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace Vacation24.Controllers
{
    public class SidebarController : Controller
    {
        //
        // GET: /Sidebar/

        public ActionResult Right()
        {
            var isLogged = ViewBag.IsLogged = ViewBag.IsOwner = WebSecurity.IsAuthenticated;

            if (isLogged)
                ViewBag.IsOwner = Roles.GetRolesForUser().Contains("owner");

            return View();
        }

        public ActionResult Left()
        {
            return View();
        }

    }
}
