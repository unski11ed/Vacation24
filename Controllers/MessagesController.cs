using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;
using Vacation24.Core.Mailer.Concrete;
using Vacation24.Models;
using Vacation24.Models.DTO;
using WebMatrix.WebData;

namespace Vacation24.Controllers
{
    public class MessagesController : CustomController
    {
        private DefaultContext _dbContext = new DefaultContext();
        private IContactFormMail _contactFormMail;
        private IContactFormMailConfirmation _contactFormMailConfirmation;

        public MessagesController(IContactFormMail contactFormMail, IContactFormMailConfirmation contatFormMailConfirmation)
        {
            _contactFormMail = contactFormMail;
            _contactFormMailConfirmation = contatFormMailConfirmation;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Send(ContactMessage message)
        {
            var ownerId = _dbContext.Places.Find(message.ObjectId).OwnerId;
            var ownerUser = _dbContext.Profiles.Where(p => p.UserId == ownerId).First();

            _contactFormMail.Email = message.Email;
            _contactFormMailConfirmation.Email = ownerUser.Email;

            _contactFormMailConfirmation.Subject = _contactFormMail.Subject = message.Subject;
            _contactFormMailConfirmation.Content = _contactFormMail.Content = message.Content.Replace("\r\n", "<br />");

            _contactFormMail.Send(ownerUser.Email);
            _contactFormMailConfirmation.Send(message.Email);

            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success, Message = "Pomyślnie wysłano wiadomość na adres Email własciciela." });
        }
    }
}
