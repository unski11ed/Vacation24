using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;

namespace Vacation24.Controllers
{
    public class HomeController : CustomController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Other(string view)
        {
            return View(view);
        }
    }
}
