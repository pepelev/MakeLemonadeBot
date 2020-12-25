using System.Threading;
using System.Threading.Tasks;

namespace FunBot.Jobs
{
    public sealed class Locking : Job
    {
        private readonly SemaphoreSlim @lock;
        private readonly CancellationToken token;
        private readonly Job job;

        public Locking(CancellationToken token, SemaphoreSlim @lock, Job job)
        {
            this.job = job;
            this.@lock = @lock;
            this.token = token;
        }

        public override async Task RunAsync()
        {
            await @lock.WaitAsync(token);
            try
            {
                await job.RunAsync();
            }
            finally
            {
                @lock.Release();
            }
        }
    }
}