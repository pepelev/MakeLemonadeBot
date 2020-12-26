using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Jobs;
using FunBot.Sheets;
using Serilog;

namespace FunBot.Updates
{
    public sealed class SheetDownloading : Job
    {
        private readonly ILogger log;
        private readonly Sheet sheet;
        private readonly CancellationToken token;
        private readonly Job<IReadOnlyList<Row>> next;

        public SheetDownloading(
            ILogger log,
            Sheet sheet,
            CancellationToken token,
            Job<IReadOnlyList<Row>> next)
        {
            this.log = log;
            this.sheet = sheet;
            this.token = token;
            this.next = next;
        }

        public override async Task RunAsync()
        {
            var rows = await sheet.RowsAsync(token);
            if (rows.Count == 0)
            {
                log.Warning("Sheet is empty");
                return;
            }

            await next.RunAsync(rows);
        }
    }
}