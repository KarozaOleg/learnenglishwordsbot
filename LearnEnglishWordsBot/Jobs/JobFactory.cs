using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace LearnEnglishWordsBot.Jobs
{
    public class JobFactory : IJobFactory
    {
        protected readonly IServiceScope _scope;

        public JobFactory(IServiceProvider container)
        {
            _scope = container.CreateScope();
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
