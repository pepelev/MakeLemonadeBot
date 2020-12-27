using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FunBot.Jobs;
using FunBot.Sheets;
using Serilog;

namespace FunBot.Updates
{
    public sealed class SerialsParsing : Job<IReadOnlyList<Row>>
    {
        private readonly ILogger log;
        private readonly Job<IReadOnlyList<Serial>> next;

        public SerialsParsing(ILogger log, Job<IReadOnlyList<Serial>> next)
        {
            this.log = log;
            this.next = next;
        }

        public override async Task RunAsync(IReadOnlyList<Row> rows)
        {
            var header = rows[0];
            var id = header.Find("Идентификатор");
            var name = header.Find("Название");
            var originalName = header.Find("Оригинальное название");
            var year = header.Find("Год");
            var duration = header.Find("Продолжительность");

            var allFound = id.Found && name.Found && originalName.Found && year.Found && duration.Found;
            if (!allFound)
            {
                log.Warning("Could not find header");
                return;
            }

            var serials = Parse();
            await next.RunAsync(serials);

            List<Serial> Parse()
            {
                var result = new List<Serial>();
                for (var i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row.Has(id) && row.Has(name) && row.Has(originalName) && row.Has(year) && row.Has(duration))
                    {
                        var serialDuration = row.Get(duration).ToLowerInvariant() switch
                        {
                            "короткий" => SerialDuration.Short,
                            "длинный" => SerialDuration.Long,
                            _ => throw new NotImplementedException()
                        };
                        var serial = new Serial(
                            row.Get(id),
                            row.Get(name),
                            row.Get(originalName),
                            int.Parse(row.Get(year), CultureInfo.InvariantCulture),
                            serialDuration
                        );
                        result.Add(serial);
                    }
                    else
                    {
                        log.Warning("Row {Index} has no id, name, original name, year, or duration", i);
                    }
                }

                return result;
            }
        }
    }
}