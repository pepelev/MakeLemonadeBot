﻿using System;
using System.Threading.Tasks;

namespace MakeLemonadeBot.Jobs
{
    public sealed class Catching : Job
    {
        private readonly Job job;

        public Catching(Job job)
        {
            this.job = job;
        }

        public override async Task RunAsync()
        {
            try
            {
                await job.RunAsync();
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
        }
    }
}