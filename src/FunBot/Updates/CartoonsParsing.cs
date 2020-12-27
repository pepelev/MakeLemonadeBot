using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FunBot.Jobs;
using FunBot.Sheets;
using Serilog;

namespace FunBot.Updates
{
    public sealed class CartoonsParsing : Job<IReadOnlyList<Row>>
    {
        private readonly ILogger log;
        private readonly Job<IReadOnlyList<Cartoon>> next;

        public CartoonsParsing(ILogger log, Job<IReadOnlyList<Cartoon>> next)
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
            var note = header.Find("Примечание");

            var allFound = id.Found && name.Found && originalName.Found && year.Found && note.Found;
            if (!allFound)
            {
                log.Warning("Could not find header");
                return;
            }

            var cartoons = Parse();
            await next.RunAsync(cartoons);

            List<Cartoon> Parse()
            {
                var result = new List<Cartoon>();
                for (var i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row.Has(id) && row.Has(name) && row.Has(year))
                    {
                        var cartoonName = row.Get(name);
                        var cartoon = new Cartoon(
                            row.Get(id),
                            cartoonName,
                            row.TryGet(originalName) ?? cartoonName,
                            int.Parse(row.Get(year), CultureInfo.InvariantCulture),
                            row.TryGet(note)
                        );
                        result.Add(cartoon);
                    }
                    else
                    {
                        log.Warning("{$Row} at {Index} has no id, name or year", row, i);
                    }
                }

                return result;
            }
        }
    }
}