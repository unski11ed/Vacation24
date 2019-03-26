using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Vacation24.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            MailMessage mail = new MailMessage();

            // set the FROM address
            mail.From = new MailAddress("automat@wczasy24.com.pl");

            // set the RECIPIENTS
            mail.To.Add("xtc888@o2.pl");

            // enter a SUBJECT
            mail.Subject = "Set the subject of the mail here.";

            // enter the message BODY
            mail.Body = "Enter text for the e-mail here.";

            // set the mail server
            SmtpClient smtp = new SmtpClient("smtp.1and1.com");

            // send the message
            smtp.Send(mail);


            return View();
        }
    }
}