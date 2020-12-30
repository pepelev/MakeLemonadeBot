using System;
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
    public sealed class SerialsUpdate : Job<IReadOnlyList<Serial>>
    {
        private readonly ILogger log;
        private readonly CancellationToken token;
        private readonly SQLiteConnection connection;

        public SerialsUpdate(ILogger log, CancellationToken token, SQLiteConnection connection)
        {
            this.log = log;
            this.token = token;
            this.connection = connection;
        }

        public override Task RunAsync(IReadOnlyList<Serial> downloaded)
        {
            var stored = Stored();
            var comparison = Full.Join(stored, downloaded, serial => serial.Id);
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

        private IReadOnlyList<Serial> Stored() => connection.Read(@"
            SELECT id, name, original_name, year, duration
            FROM serials
            ORDER BY id",
            row => new Serial(
                row.String("id"),
                row.String("name"),
                row.String("original_name"),
                row.Int("year"),
                Enum.Parse<SerialDuration>(row.String("duration"), true)
            )
        );

        private bool Update(
            IEnumerable<(string Key, Serial Old, Serial New)> comparison,
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
                        "DELETE FROM serials WHERE id = :id",
                        ("id", key)
                    );
                    log.Information("Remove {Serial}", old);
                    continue;
                }

                if (old != @new)
                {
                    transaction.Execute(@"
                    REPLACE INTO serials (id, name, original_name, year, duration)
                    VALUES (:id, :name, :original_name, :year, :duration)",
                        ("id", @new.Id),
                        ("name", @new.Name),
                        ("original_name", @new.OriginalName),
                        ("year", @new.Year),
                        ("duration", @new.Duration.ToString("G"))
                    );

                    log.Information("Replace {OldSerial} with {NewSerial}", old, @new);
                }
            }

            return true;
        }
    }
}