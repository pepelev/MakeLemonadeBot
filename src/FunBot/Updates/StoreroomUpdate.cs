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
    public sealed class StoreroomUpdate : Job<IReadOnlyList<Thing>>
    {
        private readonly ILogger log;
        private readonly SQLiteConnection connection;
        private readonly CancellationToken token;

        public StoreroomUpdate(ILogger log, CancellationToken token, SQLiteConnection connection)
        {
            this.log = log;
            this.connection = connection;
            this.token = token;
        }

        public override Task RunAsync(IReadOnlyList<Thing> downloaded)
        {
            var stored = Stored();
            var comparison = Full.Join(stored, downloaded, thing => thing.Id);
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

        private IReadOnlyList<Thing> Stored() => connection.Read(@"
            SELECT id, content, description, category
            FROM `storeroom`
            ORDER BY id",
            row => new Thing(
                row.String("id"),
                row.String("content"),
                row.MaybeString("description"),
                row.MaybeString("category")
            )
        );

        private bool Update(
            IEnumerable<(string Key, Thing Old, Thing New)> comparison,
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
                        @"DELETE FROM storeroom WHERE id = :id",
                        ("id", key)
                    );
                    log.Information("Remove {Thing}", old);
                    continue;
                }

                if (old != @new)
                {
                    transaction.Execute(@"
                    REPLACE INTO storeroom (id, content, description, category)
                    VALUES (:id, :content, :description, :category)",
                        ("id", @new.Id),
                        ("content", @new.Content),
                        ("description", @new.Description),
                        ("category", @new.Category)
                    );

                    log.Information("Replace {OldThing} with {OldThing}", old, @new);
                }
            }

            return true;
        }
    }
}