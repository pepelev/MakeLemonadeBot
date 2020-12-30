using System.Linq;
using System.Threading.Tasks;

namespace MakeLemonadeBot.Jobs
{
    public sealed class Parallel : Job
    {
        private readonly Job[] jobs;

        public Parallel(params Job[] jobs)
        {
            this.jobs = jobs;
        }

        public override async Task RunAsync()
        {
            var task = Task.WhenAll(
                jobs.Select(job => job.RunAsync())
            );
            await task;
        }
    }
}