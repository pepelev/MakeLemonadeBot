using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Storage;
using Serilog;

namespace FunBot.Updates
{
    public sealed class SerialsUpdate : Job
    {
        private readonly SQLiteConnection connection;
        private readonly ILogger log;
        private readonly Sheet sheet;
        private readonly CancellationToken token;

        public SerialsUpdate(
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
            var originalName = header.Find("Оригинальное название");
            var year = header.Find("Год");
            var duration = header.Find("Продолжительность");

            var allFound = id.Found && name.Found && originalName.Found && year.Found && duration.Found;
            if (!allFound)
            {
                log.Warning("Could not find header");
                return;
            }

            var downloaded = Parse(rows, id, name, originalName, year, duration);
            var stored = Stored();
            var join = Full.Join(stored, downloaded, serial => serial.Id);
            using var transaction = connection.BeginTransaction();
            foreach (var (key, left, right) in join)
            {
                token.ThrowIfCancellationRequested();
                if (right == null)
                {
                    transaction.Execute(
                        "DELETE FROM TABLE serials WHERE id = :id",
                        ("id", key)
                    );
                    log.Information("Remove {Key}", key);
                    continue;
                }

                if (left != right)
                {
                    transaction.Execute(@"
                    REPLACE INTO serials (id, name, original_name, year, duration)
                    VALUES (:id, :name, :original_name, :year, :duration)",
                        ("id", right.Id),
                        ("name", right.Name),
                        ("original_name", right.OriginalName),
                        ("year", right.Year),
                        ("duration", right.Duration.ToString("G"))
                    );

                    log.Information("Add or update {Key}", key);
                }
            }

            transaction.Commit();
        }

        private IEnumerable<Serial> Parse(
            IReadOnlyList<Row> rows,
            Location id,
            Location name,
            Location originalName,
            Location year,
            Location duration)
        {
            var downloaded = new List<Serial>();
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
                    var book = new Serial(
                        row.Get(id),
                        row.Get(name),
                        row.Get(originalName),
                        int.Parse(row.Get(year), CultureInfo.InvariantCulture),
                        serialDuration
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
    }
}