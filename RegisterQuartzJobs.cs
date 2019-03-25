using Quartz;
using Vacation24.Core.QuartzJobs;

namespace Vacation24 {
    public class RegisterQuartzJobs {
        private readonly IScheduler scheduler;

        public RegisterQuartzJobs(IScheduler scheduler) {
            this.scheduler = scheduler;
        }
        public async void RegisterUnsubscribedObjectsDeactivator() {
            var jobDetail = JobBuilder.Create<UnsubscribedObjectsDeactivatorJob>()
                .WithIdentity("unsubscribed-objects")
                .Build();
            var jobTrigger = TriggerBuilder.Create()
                .WithIdentity("unsubscribed-object-trigger")
                .StartNow()
                .WithSimpleSchedule(x =>
                    x.WithIntervalInHours(24)
                )
                .Build();
            await scheduler.ScheduleJob(jobDetail, jobTrigger);
        }
    }
}