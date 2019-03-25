using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Core.Configuration;
using Vacation24.Core.Mailer;

namespace Vacation24.Core.Mailer.Concrete
{
    public interface IPasswordResetMail
    {
        string Name { get; set; }
        string Url { get; set; }
        void Send(string destination);
    }

    public class PasswordResetMail : IPasswordResetMail
    {
        private IMailContentCreator contentCreator;
        private IMailer mailer;
        private MailSubjects mailSubjects;

        private const string templateFileName = "PasswordReset.html";

        public PasswordResetMail(
            IMailContentCreator contentCreator,
            IMailer mailer,
            AppConfiguration configuration
        )
        {
            this.contentCreator = contentCreator;
            this.mailer = mailer;
            this.mailSubjects = configuration.MailSubjects;
        }

        public string Name { get; set; }
        public string Url { get; set; }

        public void Send(string destination)
        {
            var mailContent = contentCreator.Create(
                this.mailSubjects.PasswordReset,
                templateFileName,
                new Dictionary<string, string>()
                    {
                        {"Name", Name},
                        {"Url", Url}
                    }
            );

            mailer.Send(destination, mailContent.Subject, mailContent.Content);
        }
    }
}