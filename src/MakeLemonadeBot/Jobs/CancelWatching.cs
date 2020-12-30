using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace MakeLemonadeBot.Jobs
{
    public sealed class CancelWatching<T> : Job<T>
    {
        private readonly ILogger log;
        private readonly CancellationToken token;
        private readonly Job<T> job;

        public CancelWatching(ILogger log, CancellationToken token, Job<T> job)
        {
            this.log = log;
            this.token = token;
            this.job = job;
        }

        public override async Task RunAsync(T argument)
        {
            if (token.IsCancellationRequested)
            {
                log.Information("Job cancelled");
                return;
            }

            await job.RunAsync(argument);
        }
    }
}