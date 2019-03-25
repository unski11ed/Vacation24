using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StructureMap;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Vacation24.Core.QuartzJobs;

namespace Vacation24 {
    public class StructureMapRegistry : Registry
    {
        public StructureMapRegistry(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment
        )
        {
            For<AppConfiguration>().Use(() => new AppConfiguration(configuration));
            For<IHostingEnvironment>().Use(() => hostingEnvironment);
            For<IHttpContextAccessor>().Use<HttpContextAccessor>();
            For<ICurrentUserProvider>().Use<CurrentUserProvider>();
            For<UnsubscribedObjectsDeactivatorJob>().Use<UnsubscribedObjectsDeactivatorJob>();
        }
    }
}
