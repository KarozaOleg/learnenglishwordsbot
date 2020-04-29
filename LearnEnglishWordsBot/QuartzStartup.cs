using LearnEnglishWordsBot.Jobs;
using LearnEnglishWordsBot.Models;
using LearnEnglishWordsBot.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace LearnEnglishWordsBot
{
    public class QuartzStartup
    {
        IScheduler _scheduler;
        readonly ILogger _logger;
        readonly IServiceProvider _serviceProvider;
        readonly JobTriggersSettigns _jobTriggerSettings;

        public QuartzStartup(
            ILogger<QuartzStartup> logger,
            IServiceProvider serviceProvider,
            IOptions<JobTriggersSettigns> triggerSettings)
        {
            _logger = logger;
            _logger.LogInformation($"{nameof(QuartzStartup)} initialize");

            _serviceProvider = serviceProvider;
            _jobTriggerSettings = triggerSettings.Value;
        }

        public async Task SetStart()
        {
            try
            {
                _scheduler = await Task.Run(() => CreateScheduler());

                await _scheduler.Start();
                await CreateJobs();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Main error, exc: {exc.Message}");
            }
        }

        public void SetStop()
        {
            if (_scheduler == null)
                return;

            if (_scheduler.Shutdown(waitForJobsToComplete: true).Wait(30000))
                _scheduler = null;
            else
                _logger.LogWarning("Не удалось корректно завершить выполнение задач");
        }
        async Task<IScheduler> CreateScheduler()
        {
            var schedulerFactory = new StdSchedulerFactory();

            var scheduler = await schedulerFactory.GetScheduler();
            scheduler.JobFactory = new JobFactory(_serviceProvider);

            return scheduler;
        }

        async Task CreateJobs()
        {
            foreach (JobType jobType in Enum.GetValues(typeof(JobType)))
            {
                IJobDetail job;
                ITrigger trigger;
                switch (jobType)
                {
                    case JobType.CreateTasksToLearn:
                        trigger = ReturnTriggerFromCronInterval(_jobTriggerSettings.CreateTasksToLearn);
                        job = JobBuilder
                            .Create<CreateTasksToLearnJob>()
                            .Build();
                        break;                    

                    #region default
                    default:
                        throw new NotImplementedException(jobType.ToString());
                        #endregion
                }

                await _scheduler.ScheduleJob(job, trigger);
            }
        }

        ITrigger ReturnTriggerFromCronInterval(string interval)
        {
            return TriggerBuilder
                    .Create()
                    .StartNow()
                    .WithCronSchedule(interval)
                    .Build();
        }
    }
}
