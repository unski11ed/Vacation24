using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using BotDetect.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Rotativa.AspNetCore;
using StructureMap;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Vacation24.Core.Configuration.Images;
using Vacation24.Core.Mailer.Concrete;
using Vacation24.Core.Payment;
using Vacation24.Middleware;
using Vacation24.Models;

namespace Vacation24
{
    public class Startup
    {
        private IHostingEnvironment hostingEnvironment;
        private Container container;
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Attach SQLite
            var connection = "./data/database.db";
            services.AddDbContext<DefaultContext>(options =>
                options.UseSqlite(connection));

            // Configure DependencyInjection
            services.AddSingleton<AppConfiguration>((serviceProvider) => new AppConfiguration(Configuration));
            services.AddSingleton<ThumbnailConfig, ThumbnailConfig>();

            services.AddScoped<IActivationMail, ActivationMail>();
            services.AddScoped<IContactFormMail, ContactFormMail>();
            services.AddScoped<IContactFormMailConfirmation, ContactFormMailConfirmation>();
            services.AddScoped<IPasswordResetMail, PasswordResetMail>();
            services.AddScoped<ISiteContactFormMail, SiteContactFormMail>();
            
            services.AddTransient<ICurrentUserProvider, CurrentUserProvider>();
            services.AddTransient<IPaymentServices, PaymentServices>();
            
            services.AddSingleton<INotesContext, DefaultContext>();
            services.AddSingleton<IOrdersContext, DefaultContext>();
            services.AddSingleton<IPaymentServicesContext, DefaultContext>();
            services.AddSingleton<IUniqueViewsContext, DefaultContext>();

            // Setup Authentication
            services.AddDefaultIdentity<Profile>()
                .AddEntityFrameworkStores<DefaultContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequiredLength = 6;

                // Lockout settings.
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(365);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // Register Quartz Jobs            
            var registrationTask = this.RegisterQuartzJobs();
            //registrationTask.Start();

            // Session
            services.AddSession(options => 
            { 
                options.IdleTimeout = TimeSpan.FromDays(365); 
            });

            // Other
            services.AddAntiforgery();
            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            registrationTask.Wait();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseCaptcha(Configuration);

            // Register custom middleware
            if (container != null) {
                app.Use(container.GetInstance<CheckUserLockout>().ExecuteAsync);
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Rotativa PDF Generator
            RotativaConfiguration.Setup(env);
        }

        private async Task RegisterQuartzJobs() {
            // Register QuartzJobs
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            
            var scheduler = await factory.GetScheduler();
            var taskRegistrer = new RegisterQuartzJobs(scheduler);

            taskRegistrer.RegisterUnsubscribedObjectsDeactivator();
        }
    }
}
