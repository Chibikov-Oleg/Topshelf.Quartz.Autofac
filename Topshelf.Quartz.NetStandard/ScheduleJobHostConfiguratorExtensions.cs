using System;
using Quartz.Spi;
using Topshelf.HostConfigurators;

namespace Topshelf.Quartz.NetStandard
{
    public static class ScheduleJobHostConfiguratorExtensions
    {
        public static HostConfigurator UsingQuartzJobFactory<TJobFactory>(this HostConfigurator configurator, Func<TJobFactory> jobFactory)
            where TJobFactory : IJobFactory
        {
            _ = jobFactory ?? throw new ArgumentNullException(nameof(jobFactory));
            _ = configurator ?? throw new ArgumentNullException(nameof(configurator));

            ScheduleJobServiceConfiguratorExtensions.JobFactory = jobFactory();
            return configurator;
        }

        public static HostConfigurator UsingQuartzJobFactory<TJobFactory>(this HostConfigurator configurator)
            where TJobFactory : IJobFactory, new()
        {
            _ = configurator ?? throw new ArgumentNullException(nameof(configurator));

            return UsingQuartzJobFactory(configurator, () => new TJobFactory());
        }

        public static HostConfigurator ScheduleQuartzJobAsService(this HostConfigurator configurator, Action<QuartzConfigurator> jobConfigurator)
        {
            _ = jobConfigurator ?? throw new ArgumentNullException(nameof(jobConfigurator));
            _ = configurator ?? throw new ArgumentNullException(nameof(configurator));

            configurator.Service<NullService>(s => s.ScheduleQuartzJob(jobConfigurator).WhenStarted(p => p.Start()).WhenStopped(p => p.Stop()).ConstructUsing(settings => new NullService()));

            return configurator;
        }
    }
}
