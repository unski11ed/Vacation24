using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using StructureMap;
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

            // Attach DI Container
            container = new Container(config =>
            {
                config.AddRegistry(new StructureMapRegistry(
                    Configuration,
                    hostingEnvironment
                ));
                config.Populate(services);
            });

            // Attach SQLite
            var connection = "./data/database.db";
            services.AddDbContext<DefaultContext>(options =>
                options.UseSqlite(connection));

            // Register Quartz Jobs            
            var registrationTask = this.RegisterQuartzJobs();
            registrationTask.Start();

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
