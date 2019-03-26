using BotDetect.Web.Mvc;
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

namespace Vacation24.Controllers
{
    public class ContactController : CustomController
    {
        private readonly ISiteContactFormMail mailer;
        private readonly AppConfiguration configuration;
        private readonly CurrentUserProvider currentUserProvider;
        private readonly DefaultContext dbContext;

        private string targetSiteContactFormMail => configuration.MailingConfiguration.SenderAddress;


        public ContactController(
            ISiteContactFormMail mailer,
            AppConfiguration configuration,
            CurrentUserProvider currentUserProvider,
            DefaultContext dbContext
        )
        {
            this.mailer = mailer;
            this.configuration = configuration;
            this.currentUserProvider = currentUserProvider;
            this.dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var viewModel = new ContactFormMessage()
            {
                Email = currentUserProvider.IsAuthenticated ?
                    dbContext.Profiles.Find(currentUserProvider.UserId).Email :
                    String.Empty
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(ContactFormInitData initData)
        {
            var viewModel = new ContactFormMessage()
            {
                Subject = initData.Subject,
                Email = currentUserProvider.IsAuthenticated ?
                    dbContext.Profiles.Find(currentUserProvider.UserId).Email :
                    String.Empty
            };

            return View(viewModel);
        }

        [HttpPost]
        [CaptchaValidation("CaptchaField", "ContactFormCaptcha", "Invalid Captcha code")]
        [ValidateAntiForgeryToken]
        public ActionResult Send(ContactFormMessage message, bool captchaValid)
        {
            if (!captchaValid)
            {
                ModelState.AddModelError("CaptchaField", "Invalid Captcha code");
                return View("Index");
            }

            if (ModelState.IsValid)
            {
                mailer.Email = message.Email;
                mailer.Subject = message.Subject;
                mailer.Content = message.Content.Replace("\r\n", "<br />");

                mailer.Send(targetSiteContactFormMail);

                return View();
            }

            return View("Index");
        }

    }
}
