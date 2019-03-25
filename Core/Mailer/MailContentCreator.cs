using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Hosting;

namespace Vacation24.Core.Mailer
{
    public interface IMailContentCreator
    {
        MailContent Create(string mailSubject, string templateName, IDictionary<string, string> keyValues);
    }

    public class MailContent
    {
        public string Subject { get; set; }
        public string Content { get; set; }
    }

    public class MailContentCreator : IMailContentCreator
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private string templatesPath {
            get {
                return Path.Combine(hostingEnvironment.ContentRootPath, "html/emailTemplates");
            }
        }

        public MailContentCreator(IHostingEnvironment hostingEnvironment) {
            this.hostingEnvironment = hostingEnvironment;
        }

        public MailContent Create(
            string mailSubject,
            string templateName,
            IDictionary<string, string> keyValues
        )
        {
            var template = readTemplate(templateName);

            return new MailContent()
            {
                Subject = replaceKeyValues(mailSubject, keyValues),
                Content = replaceKeyValues(template, keyValues)
            };
        }

        private string replaceKeyValues(string input, IDictionary<string, string> keyValues)
        {
            var output = input;
            foreach (var keyval in keyValues)
            {
                output = output.Replace("{{" + keyval.Key + "}}", keyval.Value);
            }

            return output;
        }

        private string readTemplate(string templateName)
        {
            string content = "";
            var templatePath = Path.Combine(templatesPath, templateName);

            using (StreamReader fs = new StreamReader(templatePath))
            {
                content = fs.ReadToEnd();
            }

            return content;
        }
    }
}