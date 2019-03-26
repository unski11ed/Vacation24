using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vacation24.Core;
using WebMatrix.WebData;

namespace Vacation24.Controllers
{
    public class HomeController : CustomController
    {
        public ActionResult Index()
        {
            if (!WebSecurity.IsAuthenticated)
            {
                return Redirect("/Static/Maintenance");
            }

            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult Other(string view)
        {
            return View(view);
        }
    }
}
