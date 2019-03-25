using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using Vacation24.Core.Configuration;

namespace Vacation24.Core.Mailer
{
    public interface IMailer
    {
        void Send(string destination, string subject, string body, string from = null);
        void Send(IEnumerable<string> destinations, string subject, string body, string from = null);
    }

    public class Mailer : IMailer
    {
        private readonly SmtpClient smtp;
        private readonly MailConfiguration smtpConfiguration;

        public Mailer(AppConfiguration configuration)
        {
            smtp = new SmtpClient(smtpConfiguration.Host, smtpConfiguration.Port);
            smtp.Credentials = new NetworkCredential(smtpConfiguration.Login, smtpConfiguration.Password);
            smtpConfiguration = configuration.MailingConfiguration;
        }

        public void Send(string destination, string subject, string body, string from = null)
        {
            this.Send(new string[] { destination }, subject, body, from);
        }

        public void Send(IEnumerable<string> destinations, string subject, string body, string from = null)
        {
            var message = new MailMessage();
            message.From = new MailAddress(smtpConfiguration.SenderAddress, smtpConfiguration.SenderName);
            message.ReplyToList.Add(new MailAddress(from == null ? smtpConfiguration.SenderAddress : from));
            foreach (var destination in destinations)
                message.To.Add(destination);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            smtp.Send(message);
        }
    }
}