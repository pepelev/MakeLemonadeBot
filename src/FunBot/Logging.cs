using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

namespace FunBot
{
    public sealed class Logging : Job
    {
        private readonly ILogger failure;
        private readonly Job job;
        private readonly Clock clock;
        private readonly string name;
        private readonly ILogger log;

        public Logging(string name, ILogger log, Clock clock, Job job)
            : this(name, log, log, clock, job)
        {
        }

        public Logging(string name, ILogger log, ILogger failure, Clock clock, Job job)
        {
            this.name = name;
            this.log = log;
            this.failure = failure;
            this.clock = clock;
            this.job = job;
        }

        public override async Task RunAsync()
        {
            using (LogContext.PushProperty("Name", name))
            {
                var start = clock.Now;
                try
                {
                    log.Information("Job started", start);
                    await job.RunAsync();
                }
                catch (OperationCanceledException)
                {
                    log.Information("Job cancelled in {Time}", clock.Now - start);
                    return;
                }
                catch (Exception e)
                {
                    failure.Error(e, "Job failed after {Time}", clock.Now - start);
                    throw;
                }

                log.Information("Job completed in {Time}", clock.Now - start);
            }
        }
    }
}