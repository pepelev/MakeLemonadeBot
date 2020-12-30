using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using MakeLemonadeBot.Collections;
using MakeLemonadeBot.Jobs;
using MakeLemonadeBot.Storage;
using Serilog;

namespace MakeLemonadeBot.Updates
{
    public sealed class BooksUpdate : Job<IReadOnlyList<Book>>
    {
        private readonly ILogger log;
        private readonly CancellationToken token;
        private readonly SQLiteConnection connection;

        public BooksUpdate(ILogger log, CancellationToken token, SQLiteConnection connection)
        {
            this.log = log;
            this.token = token;
            this.connection = connection;
        }

        public override Task RunAsync(IReadOnlyList<Book> downloaded)
        {
            var stored = Stored();
            var comparison = Full.Join(stored, downloaded, book => book.Id);
            using var transaction = connection.BeginTransaction();
            try
            {
                if (Update(comparison, transaction))
                {
                    transaction.Commit();
                    return Task.CompletedTask;
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            transaction.Rollback();
            return Task.CompletedTask;
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

        private bool Update(
            IEnumerable<(string Key, Book Old, Book New)> comparison,
            SQLiteTransaction transaction)
        {
            foreach (var (key, old, @new) in comparison)
            {
                if (token.IsCancellationRequested)
                {
                    log.Information("Cancellation requested");
                    return false;
                }

                if (@new == null)
                {
                    transaction.Execute(
                        @"DELETE FROM books WHERE id = :id",
                        ("id", key)
                    );
                    log.Information("Remove {Book}", old);
                    continue;
                }

                if (old != @new)
                {
                    transaction.Execute(@"
                    REPLACE INTO books (id, name, author)
                    VALUES (:id, :name, :author)",
                        ("id", @new.Id),
                        ("name", @new.Name),
                        ("author", @new.Author)
                    );

                    log.Information("Replace {OldBook} with {NewBook}", old, @new);
                }
            }

            return true;
        }
    }
}