using System.Threading;
using System.Threading.Tasks;

namespace FunBot
{
    public sealed class Cycle : Job
    {
        private readonly Job job;
        private readonly CancellationToken token;

        public Cycle(Job job, CancellationToken token)
        {
            this.job = job;
            this.token = token;
        }

        public override async Task RunAsync()
        {
            while (!token.IsCancellationRequested)
            {
                await job.RunAsync();
            }
        }
    }
}