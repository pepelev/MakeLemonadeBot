using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunBot
{
    public sealed class CancelWatching : Job
    {
        private readonly Job job;
        private readonly CancellationToken token;

        public CancelWatching(Job job, CancellationToken token)
        {
            this.job = job;
            this.token = token;
        }

        public override async Task RunAsync()
        {
            token.ThrowIfCancellationRequested();
            await job.RunAsync();
        }
    }
}