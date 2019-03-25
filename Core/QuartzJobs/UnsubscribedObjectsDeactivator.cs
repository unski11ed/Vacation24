using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Vacation24.Core.QuartzJobs
{
    public class UnsubscribedObjectsDeactivatorJob : IJob
    {
        private readonly ObjectsActivator objectsActivator;

        UnsubscribedObjectsDeactivatorJob(ObjectsActivator objectsActivator) {
            this.objectsActivator = objectsActivator;
        }

        Task IJob.Execute(IJobExecutionContext context)
        {
            return new Task(() => {
                objectsActivator.ProcessObjectsToDeactivate();
            });
        }
    }
}