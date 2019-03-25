﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Core.Configuration;
using Vacation24.Core.Mailer;

namespace Vacation24.Core.Mailer.Concrete
{
    public interface ISiteContactFormMail
    {
        string Email { get; set; }
        string Subject { get; set; }
        string Content { get; set; }
        void Send(string destination);
    }

    public class SiteContactFormMail : ISiteContactFormMail
    {
        private IMailContentCreator contentCreator;
        private IMailer mailer;
        private MailSubjects mailSubjects;
        private const string templateFileName = "SiteContactFormMessage.html";

        public SiteContactFormMail(
            IMailContentCreator contentCreator,
            IMailer mailer,
            AppConfiguration configuration
        )
        {
            this.contentCreator = contentCreator;
            this.mailer = mailer;
            this.mailSubjects = configuration.MailSubjects;
        }

        public string Email { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public void Send(string destination)
        {
            var mailContent = contentCreator.Create(
                this.mailSubjects.SiteContactFormMessage,
                templateFileName,
                new Dictionary<string, string>()
                    {
                        {"Email", Email},
                        {"Subject", Subject},
                        {"Content", Content}
                    }
            );

            mailer.Send(destination, mailContent.Subject, mailContent.Content, Email);
        }
    }
}