using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StructureMap;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Vacation24.Core.Mailer.Concrete;
using Vacation24.Core.QuartzJobs;
using Vacation24.Middleware;
using Vacation24.Models;

namespace Vacation24 {
    public class StructureMapRegistry : Registry
    {
        public StructureMapRegistry(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment
        )
        {
            // Dotnet instances
            For<AppConfiguration>().Use(() => new AppConfiguration(configuration));
            For<IHostingEnvironment>().Use(() => hostingEnvironment);
            For<IHttpContextAccessor>().Use<HttpContextAccessor>();

            // EntityFramework Contexts
            For<DefaultContext>().Singleton();
            For<INotesContext>().Use<DefaultContext>().Singleton();
            For<IOrdersContext>().Use<DefaultContext>().Singleton();
            For<IPaymentServicesContext>().Use<DefaultContext>().Singleton();
            For<IUniqueViewsContext>().Use<DefaultContext>().Singleton();
            
            // Application Specific
            For<ICurrentUserProvider>().Use<CurrentUserProvider>();
            For<UnsubscribedObjectsDeactivatorJob>().Use<UnsubscribedObjectsDeactivatorJob>();

            // Mailers
            For<IActivationMail>().Use<ActivationMail>();
            For<IContactFormMail>().Use<ContactFormMail>();
            For<IContactFormMailConfirmation>().Use<ContactFormMailConfirmation>();
            For<IPasswordResetMail>().Use<PasswordResetMail>();
            For<ISiteContactFormMail>().Use<SiteContactFormMail>();

            // Middleware
            For<CheckUserLockout>().Use<CheckUserLockout>();

        }
    }
}
