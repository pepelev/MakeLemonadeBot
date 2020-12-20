using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Storage;
using Serilog;

namespace FunBot.Updates
{
    public sealed class MoviesUpdate : Job
    {
        private readonly SQLiteConnection connection;
        private readonly ILogger log;
        private readonly Sheet sheet;
        private readonly CancellationToken token;

        public MoviesUpdate(
            SQLiteConnection connection,
            ILogger log,
            Sheet sheet,
            CancellationToken token)
        {
            this.connection = connection;
            this.log = log;
            this.sheet = sheet;
            this.token = token;
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

            var allFound = id.Found && name.Found && originalName.Found && year.Found;
            if (!allFound)
            {
                log.Warning("Could not find header");
                return;
            }

            var downloaded = Parse(rows, id, name, originalName, year);
            var duplicateKeys = downloaded
                .GroupBy(movie => movie.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateKeys.Count > 0)
            {
                log.Warning("Duplicate entries with {Keys}, cancel update", duplicateKeys);
                return;
            }

            token.ThrowIfCancellationRequested();
            var stored = Stored();
            var join = Full.Join(stored, downloaded, movie => movie.Id);
            using var transaction = connection.BeginTransaction();
            foreach (var (key, left, right) in join)
            {
                token.ThrowIfCancellationRequested();
                if (right == null)
                {
                    transaction.Execute(
                        @"DELETE FROM TABLE movies WHERE id = :id",
                        ("id", key)
                    );
                    log.Information("Remove {Key}", key);
                    continue;
                }

                if (left != right)
                {
                    transaction.Execute(@"
                    REPLACE INTO movies (id, name, original_name, year)
                    VALUES (:id, :name, :original_name, :year)",
                        ("id", right.Id),
                        ("name", right.Name),
                        ("original_name", right.OriginalName),
                        ("year", right.Year)
                    );

                    log.Information("Add or update {Key}", key);
                }
            }

            transaction.Commit();
        }

        private List<Movie> Parse(
            IReadOnlyList<Row> rows,
            Location id,
            Location name,
            Location originalName,
            Location year)
        {
            var downloaded = new List<Movie>();
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
                    downloaded.Add(movie);
                }
                else
                {
                    log.Warning("Row {Index} has no id or name", i);
                }
            }

            return downloaded;
        }

        private IReadOnlyList<Movie> Stored() => connection.Read(@"
            SELECT id, name, original_name, year
            FROM `movies`
            ORDER BY id",
            row => new Movie(
                row.String("id"),
                row.String("name"),
                row.MaybeString("original_name"),
                row.MaybeInt("year")
            )
        );
    }
}