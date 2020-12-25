using System.Threading.Tasks;

namespace FunBot.Jobs
{
    public sealed class Sequential : Job
    {
        private readonly Job[] jobs;

        public Sequential(params Job[] jobs)
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