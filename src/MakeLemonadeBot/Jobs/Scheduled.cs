using System;
using System.Threading;
using System.Threading.Tasks;

namespace MakeLemonadeBot.Jobs
{
    public sealed class Scheduled : Job
    {
        private readonly Clock clock;
        private readonly Job job;
        private readonly DateTime offset;
        private readonly TimeSpan period;
        private readonly CancellationToken token;

        public Scheduled(DateTime offset, TimeSpan period, CancellationToken token, Clock clock, Job job)
        {
            if (period <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(period), period, "Must be positive");

            this.offset = offset;
            this.period = period;
            this.token = token;
            this.clock = clock;
            this.job = job;
        }

        public override async Task RunAsync()
        {
            var now = clock.Now;
            var span = Span(now);
            var delay = period - span;
            await Task.Delay(delay, token);
            await job.RunAsync();
        }

        private TimeSpan Span(DateTime now)
        {
            checked
            {
                var span = now - offset;
                if (span >= TimeSpan.Zero)
                    return new TimeSpan(span.Ticks % period.Ticks);

                var times = -span.Ticks % period.Ticks;
                return span + new TimeSpan(period.Ticks * (times + 1));
            }
        }
    }
}