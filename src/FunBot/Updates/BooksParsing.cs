using System.Collections.Generic;
using System.Threading.Tasks;
using FunBot.Jobs;
using FunBot.Sheets;
using Serilog;

namespace FunBot.Updates
{
    public sealed class BooksParsing : Job<IReadOnlyList<Row>>
    {
        private readonly ILogger log;
        private readonly Job<IReadOnlyList<Book>> next;

        public BooksParsing(ILogger log, Job<IReadOnlyList<Book>> next)
        {
            this.log = log;
            this.next = next;
        }

        public override async Task RunAsync(IReadOnlyList<Row> rows)
        {
            var header = rows[0];
            var id = header.Find("Идентификатор");
            var name = header.Find("Название");
            var author = header.Find("Автор");

            var allFound = id.Found && name.Found && author.Found;
            if (!allFound)
            {
                log.Warning("Could not find header");
                return;
            }

            var books = Parse();
            await next.RunAsync(books);

            List<Book> Parse()
            {
                var result = new List<Book>();
                for (var i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row.Has(id) && row.Has(name) && row.Has(author))
                    {
                        var book = new Book(
                            row.Get(id),
                            row.Get(name),
                            row.Get(author)
                        );
                        result.Add(book);
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