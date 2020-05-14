using System;
using System.Collections.Generic;
using Quartz;

namespace Topshelf.Quartz.NetStandard
{
    public class QuartzConfigurator
    {
        public QuartzConfigurator()
        {
            Triggers = new List<Func<ITrigger>>();
        }

        public Func<IJobDetail>? Job { get; set; }

        public IList<Func<ITrigger>> Triggers { get; }

        public Func<bool> JobEnabled { get; set; } = () => true;

        public QuartzConfigurator WithJob(Func<IJobDetail> jobDetail)
        {
            Job = jobDetail;
            return this;
        }

        public QuartzConfigurator AddTrigger(Func<ITrigger> jobTrigger)
        {
            Triggers.Add(jobTrigger);
            return this;
        }

        public QuartzConfigurator EnableJobWhen(Func<bool> jobEnabled)
        {
            JobEnabled = jobEnabled;
            return this;
        }
    }
}
