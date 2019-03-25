using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Core.Configuration;
using Vacation24.Core.Mailer;

namespace Vacation24.Core.Mailer.Concrete
{
    public interface IActivationMail
    {
        string Name { get; set; }
        string Url { get; set; }
        void SendSeller(string destination);
        void SendUser(string destination);
    }

    public class ActivationMail : IActivationMail
    {
        private IMailContentCreator contentCreator;
        private IMailer mailer;
        private MailSubjects mailSubjects;
        
        public ActivationMail(IMailContentCreator contentCreator, IMailer mailer, AppConfiguration configuration)
        {
            this.contentCreator = contentCreator;
            this.mailer = mailer;
            this.mailSubjects = configuration.MailSubjects;
        }

        public string Name { get; set; }
        public string Url { get; set; }

        public void SendUser(string destination)
        {
            var template = contentCreator.Create(
                mailSubjects.AccountConfirmationUser,
                "AccountConfirmationUser.html",
                new Dictionary<string, string>()
                {
                    {"Name", Name},
                    {"Url", Url}
                }
            );

            mailer.Send(destination, template.Subject, template.Content);
        }
        public void SendSeller(string destination)
        {
            var template = contentCreator.Create(
                mailSubjects.AccountConfirmationSeller,
                "AccountConfirmationSeller.html",
                new Dictionary<string, string>()
                {
                    {"Name", Name},
                    {"Url", Url}
                }
            );

            mailer.Send(destination, template.Subject, template.Content);
        }
    }
}