﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace Vacation24.Core.Configuration
{
    public class ThumbnailSizes {
        public Size Small { get; set; }
        public Size Medium { get; set; }
        public Size Large { get; set; }
        public Size ExtraLarge { get;set; }
        public Size Mega { get; set; }
    }
    public class ImagesConfiguration
    {
        public int PhotoMaxWidth { get; set; }
        public int PhotoMaxHeight { get; set; }
        public ThumbnailSizes ThumbnailSizes { get; set;}
        public string PhotosPath { get; set; }
        public string ThumbnailPath { get; set; }
        public string UploadUrl { get; set; }
        public string DeleteUrl { get; set; }
        public string GetThumbnailsUrl { get; set; }
        public string PhotosUrl { get; set; }
        public string ThumbnailUrl { get; set; }

    }
    public class MailConfiguration
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get { return port; } set { port = value; } }
        public string SenderAddress { get; set; }
        public string SenderName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        //Default port value
        private int port = 25;
    }

    public class MailSubjects {
        public string AccountConfirmationSeller { get; set; }
        public string AccountConfirmationUser { get; set; }
        public string ContactFormMessage { get; set; }
        public string ContactFormMessageConfirmation { get; set; }
        public string PasswordReset { get; set; }
        public string SiteContactFormMessage { get; set; }
    }

    public class PayUConfiguration
    {
        public string ApiUrl { get; set; }
        public string User { get; set; }
        public string Key { get; set; }
    }

    public class SiteConfiguration
    {
        public string RootPath { get; set; }

        public int CommentsPerPage { get; set; }

        public string ContactFormReceiver { get; set; }
    }

    public class RecaptchaConfiguration
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }

    public class ApiKeys
    {
        public string GoogleMap { get; set; }
    }

    public class AppConfiguration
    {
        public string SqliteDatabaseFile { get; set; }
        public ImagesConfiguration ImagesConfiguration { get; set; }
        public SiteConfiguration SiteConfiguration { get; set; }
        public PayUConfiguration PayUConfiguration { get; set; }
        public RecaptchaConfiguration RecaptchaConfiguration { get; set; }
        public MailConfiguration MailingConfiguration { get; set; }
        public MailSubjects MailSubjects { get; set; }
        public ApiKeys ApiKeys { get; set; }

        public AppConfiguration(IConfiguration configuration) {
            this.SiteConfiguration = configuration.GetSection("SiteConfiguration") as SiteConfiguration;
            this.ImagesConfiguration = configuration.GetSection("ImagesConfiguration") as ImagesConfiguration;
            this.PayUConfiguration = configuration.GetSection("PayUConfiguration") as PayUConfiguration;
            this.RecaptchaConfiguration = configuration.GetSection("RecaptchaConfiguration") as RecaptchaConfiguration;
            this.MailingConfiguration = configuration.GetSection("MailConfiguration") as MailConfiguration;
            this.MailSubjects = configuration.GetSection("MailSubjects") as MailSubjects;
            this.ApiKeys = configuration.GetSection("ApiKeys") as ApiKeys;
        }
    }
}