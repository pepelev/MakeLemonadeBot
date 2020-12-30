using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MakeLemonadeBot.Jobs
{
    public sealed class Sequential : Job
    {
        private readonly IEnumerable<Job> jobs;

        public Sequential(params Job[] jobs)
            : this(jobs.AsEnumerable())
        {
        }

        public Sequential(IEnumerable<Job> jobs)
        {
            this.jobs = jobs;
        }

        public override async Task RunAsync()
        {
            foreach (var job in jobs)
            {
                await job.RunAsync();
            }
        }
    }
}