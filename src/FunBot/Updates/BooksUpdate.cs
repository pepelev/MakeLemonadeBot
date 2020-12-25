using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Collections;
using FunBot.Jobs;
using FunBot.Sheets;
using FunBot.Storage;
using Serilog;

namespace FunBot.Updates
{
    public sealed class BooksUpdate : Job
    {
        private readonly SQLiteConnection connection;
        private readonly ILogger log;
        private readonly Sheet sheet;
        private readonly CancellationToken token;

        public BooksUpdate(
            SQLiteConnection connection,
            ILogger log,
            Sheet sheet,
            CancellationToken token)
        {
            this.connection = connection;
            this.log = log;
            this.token = token;
            this.sheet = sheet;
        }

        public override async Task RunAsync()
        {
            var rows = await sheet.RowsAsync(token);
            if (rows.Count == 0)
            {
                log.Warning("Sheet is empty");
                return;
            }

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

            var downloaded = Parse(rows, id, name, author);
            var stored = Stored();
            var join = Full.Join(stored, downloaded, book => book.Id);
            using var transaction = connection.BeginTransaction();
            foreach (var (key, left, right) in join)
            {
                token.ThrowIfCancellationRequested();
                if (right == null)
                {
                    transaction.Execute(
                        @"DELETE FROM TABLE books WHERE id = :id",
                        ("id", key)
                    );
                    log.Information("Remove {Key}", key);
                    continue;
                }

                if (left != right)
                {
                    transaction.Execute(@"
                    REPLACE INTO books (id, name, author)
                    VALUES (:id, :name, :author)",
                        ("id", right.Id),
                        ("name", right.Name),
                        ("author", right.Author)
                    );

                    log.Information("Add or update {Key}", key);
                }
            }

            transaction.Commit();
        }

        private List<Book> Parse(
            IReadOnlyList<Row> rows,
            Location id,
            Location name,
            Location author)
        {
            var downloaded = new List<Book>();
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
                    downloaded.Add(book);
                }
                else
                {
                    log.Warning("Row {Index} has no id, name or author", i);
                }
            }

            return downloaded;
        }

        private IReadOnlyList<Book> Stored() => connection.Read(@"
            SELECT id, name, author
            FROM books
            ORDER BY id",
            row => new Book(
                row.String("id"),
                row.String("name"),
                row.String("author")
            )
        );
    }
}