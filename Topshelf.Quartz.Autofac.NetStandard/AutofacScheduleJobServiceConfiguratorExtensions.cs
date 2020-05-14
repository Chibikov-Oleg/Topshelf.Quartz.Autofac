using System;
using Autofac;
using Quartz;
using Topshelf.Logging;
using Topshelf.Quartz.NetStandard;
using Topshelf.ServiceConfigurators;

namespace Topshelf.Quartz.Autofac.NetStandard
{
    /// <summary>
    /// Allows to use Quartz with Autofac within topshelf configurator.
    /// </summary>
    public static class AutofacScheduleJobServiceConfiguratorExtensions
    {
        /// <summary>
        /// Bind Quartz to Topshelf service configurator and Autofac.
        /// </summary>
        /// <param name="configurator">Topshelf service configurator.</param>
        /// <param name="lifetimeScope">Autofac lifetime scope.</param>
        /// <typeparam name="T">Type of host.</typeparam>
        /// <returns>Service configurator for Topshelf.</returns>
        public static ServiceConfigurator<T> UseQuartzAutofac<T>(this ServiceConfigurator<T> configurator, ILifetimeScope lifetimeScope)
            where T : class
        {
            _ = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            _ = configurator ?? throw new ArgumentNullException(nameof(configurator));
            SetupAutofac(lifetimeScope);
            return configurator;
        }

        internal static void SetupAutofac(ILifetimeScope container)
        {
            _ = container ?? throw new ArgumentNullException(nameof(container));

            Func<IScheduler> schedulerFactory = container.Resolve<IScheduler>;
            ScheduleJobServiceConfiguratorExtensions.SchedulerFactory = schedulerFactory;
            var log = HostLogger.Get(typeof(AutofacScheduleJobServiceConfiguratorExtensions));
            log.Info("[Topshelf.Quartz.Autofac] Quartz configured to construct jobs with Autofac.");
        }
    }
}
