using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Vacation24.Core.Mailer.Concrete;
using Vacation24.Models;

namespace Vacation24.Controllers
{
    public class MessagesController : CustomController
    {
        private readonly IContactFormMail contactFormMail;
        private readonly IContactFormMailConfirmation contactFormMailConfirmation;
        private readonly DefaultContext dbContext;

        public MessagesController(
            IContactFormMail contactFormMail,
            IContactFormMailConfirmation contatFormMailConfirmation,
            DefaultContext dbContext
        )
        {
            this.contactFormMail = contactFormMail;
            this.contactFormMailConfirmation = contatFormMailConfirmation;
            this.dbContext = dbContext;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Send(ContactMessage message)
        {
            var ownerId = dbContext.Places.Find(message.ObjectId).OwnerId;
            var ownerUser = dbContext.Profiles.Find(ownerId);

            contactFormMail.Email = message.Email;
            contactFormMailConfirmation.Email = ownerUser.Email;

            contactFormMailConfirmation.Subject = contactFormMail.Subject = message.Subject;
            contactFormMailConfirmation.Content = contactFormMail.Content = message.Content.Replace("\r\n", "<br />");

            contactFormMail.Send(ownerUser.Email);
            contactFormMailConfirmation.Send(message.Email);

            return Json(
                new ResultViewModel()
                    {
                        Status = (int)ResultStatus.Success,
                        Message = "Successfully sent a message to owners Email address."
                    }
            );
        }
    }
}
