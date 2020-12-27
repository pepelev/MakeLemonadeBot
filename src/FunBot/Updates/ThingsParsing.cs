using System.Collections.Generic;
using System.Threading.Tasks;
using FunBot.Jobs;
using FunBot.Sheets;
using Serilog;

namespace FunBot.Updates
{
    public sealed class ThingsParsing : Job<IReadOnlyList<Row>>
    {
        private readonly ILogger log;
        private readonly Job<IReadOnlyList<Thing>> next;

        public ThingsParsing(ILogger log, Job<IReadOnlyList<Thing>> next)
        {
            this.log = log;
            this.next = next;
        }

        public override async Task RunAsync(IReadOnlyList<Row> rows)
        {
            var header = rows[0];
            var id = header.Find("Идентификатор");
            var content = header.Find("Ссылка");
            var description = header.Find("Описание");
            var category = header.Find("Категория");

            var allFound = id.Found && content.Found && description.Found && category.Found;
            if (!allFound)
            {
                log.Warning("Could not find header");
                return;
            }

            var things = Parse();
            await next.RunAsync(things);

            List<Thing> Parse()
            {
                var result = new List<Thing>();
                for (var i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row.Has(id) && row.Has(content))
                    {
                        var thing = new Thing(
                            row.Get(id),
                            row.Get(content),
                            row.TryGet(description),
                            row.TryGet(category)
                        );
                        result.Add(thing);
                    }
                    else
                    {
                        log.Warning("{$Row} at {Index} has no id or content", row, i);
                    }
                }

                return result;
            }
        }
    }
}