using System;
using Autofac;
using Topshelf.HostConfigurators;

namespace Topshelf.Quartz.Autofac.NetStandard
{
    /// <summary>
    /// Allows to use Quartz with Autofac within topshelf configurator.
    /// </summary>
    public static class AutofacScheduleJobHostConfiguratorExtensions
    {
        /// <summary>
        /// Bind Quartz to Topshelf host configurator and Autofac.
        /// </summary>
        /// <param name="configurator">Topshelf host configurator.</param>
        /// <param name="lifetimeScope">Autofac lifetime scope.</param>
        /// <returns>Host configurator for Topshelf.</returns>
        public static HostConfigurator UseQuartzAutofac(this HostConfigurator configurator, ILifetimeScope lifetimeScope)
        {
            _ = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            _ = configurator ?? throw new ArgumentNullException(nameof(configurator));

            AutofacScheduleJobServiceConfiguratorExtensions.SetupAutofac(lifetimeScope);
            return configurator;
        }
    }
}
