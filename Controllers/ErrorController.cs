using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Vacation24.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View("Error");
        }

        /*
        public override ActionResult NotFound()
        {
            Response.StatusCode = 404;

            return View("NotFound");
        }

        public override ActionResult Unauthorized()
        {
            Response.StatusCode = 403;

            return View("Unauthorized");
        }
        */
    }
}