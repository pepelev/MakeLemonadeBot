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
    public sealed class CartoonsUpdate : Job<IReadOnlyList<Cartoon>>
    {
        private readonly ILogger log;
        private readonly SQLiteConnection connection;
        private readonly CancellationToken token;

        public CartoonsUpdate(ILogger log, CancellationToken token, SQLiteConnection connection)
        {
            this.log = log;
            this.connection = connection;
            this.token = token;
        }

        public override Task RunAsync(IReadOnlyList<Cartoon> downloaded)
        {
            var stored = Stored();
            var comparison = Full.Join(stored, downloaded, cartoon => cartoon.Id);
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

        private IReadOnlyList<Cartoon> Stored() => connection.Read(@"
            SELECT id, name, original_name, year, note
            FROM `cartoons`
            ORDER BY id",
            row => new Cartoon(
                row.String("id"),
                row.String("name"),
                row.String("original_name"),
                row.Int("year"),
                row.MaybeString("note")
            )
        );

        private bool Update(
            IEnumerable<(string Key, Cartoon Old, Cartoon New)> comparison,
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
                        @"DELETE FROM cartoons WHERE id = :id",
                        ("id", key)
                    );
                    log.Information("Remove {Cartoon}", old);
                    continue;
                }

                if (old != @new)
                {
                    transaction.Execute(@"
                    REPLACE INTO cartoons (id, name, original_name, year, note)
                    VALUES (:id, :name, :original_name, :year, :note)",
                        ("id", @new.Id),
                        ("name", @new.Name),
                        ("original_name", @new.OriginalName),
                        ("year", @new.Year),
                        ("note", @new.Note)
                    );

                    log.Information("Replace {OldCartoon} with {NewCartoon}", old, @new);
                }
            }

            return true;
        }
    }
}