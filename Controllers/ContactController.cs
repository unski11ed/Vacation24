using BotDetect.Web.UI.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Vacation24.Core.Mailer.Concrete;
using Vacation24.Models;
using Vacation24.Models.DTO;
using WebMatrix.WebData;

namespace Vacation24.Controllers
{
    public class ContactController : CustomController
    {
        private ISiteContactFormMail _mailer;
        private DefaultContext _dbContext = new DefaultContext();

        private string TargetSiteContactFormMail
        {
            get
            {
                return AppConfiguration.Get().SiteConfiguration.ContactFormReceiver;
            }
        }


        public ContactController(ISiteContactFormMail mail)
        {
            _mailer = mail;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var viewModel = new ContactFormMessage()
            {
                Email = WebSecurity.IsAuthenticated ? WebSecurity.CurrentUserName : ""
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(ContactFormInitData initData)
        {
            var viewModel = new ContactFormMessage()
            {
                Subject = initData.Subject,
                Email = WebSecurity.IsAuthenticated ? WebSecurity.CurrentUserName : ""
            };

            return View(viewModel);
        }

        [HttpPost]
        [CaptchaValidation("CaptchaField", "ContactFormCaptcha", "Nieprawidłowy kod bezpieczeńśtwa")]
        [ValidateAntiForgeryToken]
        public ActionResult Send(ContactFormMessage message, bool captchaValid)
        {
            if (!captchaValid)
            {
                ModelState.AddModelError("CaptchaField", "Nieprawidłowy kod bezpieczeństwa");
                return View("Index");
            }

            if (ModelState.IsValid)
            {
                _mailer.Email = message.Email;
                _mailer.Subject = message.Subject;
                _mailer.Content = message.Content.Replace("\r\n", "<br />");

                _mailer.Send(TargetSiteContactFormMail);

                return View();
            }

            return View("Index");
        }

    }
}
