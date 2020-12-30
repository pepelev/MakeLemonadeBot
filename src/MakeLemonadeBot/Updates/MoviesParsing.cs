using System.Collections.Generic;
using System.Threading.Tasks;
using MakeLemonadeBot.Jobs;
using MakeLemonadeBot.Sheets;
using Serilog;

namespace MakeLemonadeBot.Updates
{
    public sealed class MoviesParsing : Job<IReadOnlyList<Row>>
    {
        private readonly ILogger log;
        private readonly Job<IReadOnlyList<Movie>> next;

        public MoviesParsing(ILogger log, Job<IReadOnlyList<Movie>> next)
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

            var allFound = id.Found && name.Found && originalName.Found && year.Found;
            if (!allFound)
            {
                log.Warning("Could not find header");
                return;
            }

            var movies = Parse();
            await next.RunAsync(movies);

            List<Movie> Parse()
            {
                var result = new List<Movie>();
                for (var i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row.Has(id) && row.Has(name))
                    {
                        var movie = new Movie(
                            row.Get(id),
                            row.Get(name),
                            row.TryGet(originalName),
                            row.TryGet(year).AsNumber()
                        );
                        result.Add(movie);
                    }
                    else
                    {
                        log.Warning("{$Row} at {Index} has no id or name", row, i);
                    }
                }

                return result;
            }
        }
    }
}