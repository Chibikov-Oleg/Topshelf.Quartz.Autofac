using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Topshelf.Logging;
using Topshelf.ServiceConfigurators;

namespace Topshelf.Quartz.NetStandard
{
    public static class ScheduleJobServiceConfiguratorExtensions
    {
        static readonly Func<IScheduler> DefaultSchedulerFactory = () =>
        {
            var schedulerFactory = new StdSchedulerFactory();
            return schedulerFactory.GetScheduler().Result;
        };

        static Func<IScheduler>? _customSchedulerFactory;
        static IScheduler? _scheduler;

        public static Func<IScheduler> SchedulerFactory
        {
            get => _customSchedulerFactory ?? DefaultSchedulerFactory;
            set => _customSchedulerFactory = value;
        }

        internal static IJobFactory? JobFactory { get; set; }

        public static ServiceConfigurator<T> UsingQuartzJobFactory<T, TJobFactory>(this ServiceConfigurator<T> configurator, Func<TJobFactory> jobFactory)
            where T : class
            where TJobFactory : IJobFactory
        {
            _ = jobFactory ?? throw new ArgumentNullException(nameof(jobFactory));

            JobFactory = jobFactory();
            return configurator;
        }

        public static ServiceConfigurator<T> UsingQuartzJobFactory<T, TJobFactory>(this ServiceConfigurator<T> configurator)
            where T : class
            where TJobFactory : IJobFactory, new()
        {
            _ = configurator ?? throw new ArgumentNullException(nameof(configurator));

            return UsingQuartzJobFactory(configurator, () => new TJobFactory());
        }

        public static ServiceConfigurator<T> ScheduleQuartzJob<T>(this ServiceConfigurator<T> configurator, Action<QuartzConfigurator> jobConfigurator)
            where T : class
        {
            _ = jobConfigurator ?? throw new ArgumentNullException(nameof(jobConfigurator));
            _ = configurator ?? throw new ArgumentNullException(nameof(configurator));

            ConfigureJob(configurator, jobConfigurator);
            return configurator;
        }

        static IScheduler GetScheduler()
        {
            var scheduler = SchedulerFactory();

            if (JobFactory != null)
            {
                scheduler.JobFactory = JobFactory;
            }

            return scheduler;
        }

        static void ConfigureJob<T>(ServiceConfigurator<T> configurator, Action<QuartzConfigurator> jobConfigurator)
            where T : class
        {
            var log = HostLogger.Get(typeof(ScheduleJobServiceConfiguratorExtensions));

            var jobConfig = new QuartzConfigurator();
            jobConfigurator(jobConfig);

            if ((jobConfig.Job != null) && ((jobConfig.JobEnabled == null) || jobConfig.JobEnabled() || (jobConfig.Triggers == null)))
            {
                var jobDetail = jobConfig.Job();
                var jobTriggers = jobConfig.Triggers.Select(triggerFactory => triggerFactory()).Where(trigger => trigger != null).ToArray();

                configurator.BeforeStartingService(
                    () =>
                    {
                        log.Debug("[Topshelf.Quartz] Scheduler starting up...");
                        if (_scheduler == null)
                        {
                            _scheduler = GetScheduler();
                        }

                        if ((_scheduler != null) && (jobDetail != null) && (jobTriggers.Length > 0))
                        {
                            var triggersForJob = new HashSet<ITrigger>(jobTriggers);
                            _scheduler.ScheduleJob(jobDetail, triggersForJob, false);
                            log.InfoFormat(CultureInfo.InvariantCulture, "[Topshelf.Quartz] Scheduled Job: {0}", jobDetail.Key);

                            foreach (var trigger in triggersForJob)
                            {
                                var nextFireTime = trigger.GetNextFireTimeUtc();
                                log.InfoFormat(
                                    CultureInfo.InvariantCulture,
                                    "[Topshelf.Quartz] Job Schedule: {0} - Next Fire Time (local): {1}",
                                    trigger,
                                    nextFireTime != null ? nextFireTime.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture) : "none");
                            }

                            _scheduler.Start();
                            log.Info("[Topshelf.Quartz] Scheduler started...");
                        }
                    });

                configurator.BeforeStoppingService(
                    () =>
                    {
                        log.Debug("[Topshelf.Quartz] Scheduler shutting down...");
                        _scheduler?.Shutdown(true);
                        log.Info("[Topshelf.Quartz] Scheduler shut down...");
                    });
            }
        }
    }
}
