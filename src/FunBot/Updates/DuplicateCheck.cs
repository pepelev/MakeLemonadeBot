using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunBot.Jobs;
using Serilog;

namespace FunBot.Updates
{
    public sealed class DuplicateCheck<T> : Job<IReadOnlyList<T>>
    {
        private readonly ILogger log;
        private readonly Func<T, string> id;
        private readonly Job<IReadOnlyList<T>> next;

        public DuplicateCheck(ILogger log, Func<T, string> id, Job<IReadOnlyList<T>> next)
        {
            this.log = log;
            this.id = id;
            this.next = next;
        }

        public override async Task RunAsync(IReadOnlyList<T> list)
        {
            var duplicateKeys = list
                .GroupBy(id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateKeys.Count > 0)
            {
                log.Warning("Duplicate entries with {Keys}, cancel update", duplicateKeys);
                return;
            }

            await next.RunAsync(list);
        }
    }
}