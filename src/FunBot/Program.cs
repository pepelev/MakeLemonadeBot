using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FunBot
{
    class Program
    {
        private const string spreadsheetId = "spread";
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "FunBot";

        static async Task Main2(string[] args)
        {
            var connection = new SQLiteConnection("Data Source = fun-bot.sqlite; Version = 3;");
            connection.Open();

            using var initialCreateTable = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS `movies` (
                    id TEXT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    original_name TEXT,
                    year INT
                )",
                connection
            );
            initialCreateTable.ExecuteNonQuery();

            var configuration = new LoggerConfiguration();
            using var log = configuration
                .WriteTo.File("log.txt")
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .CreateLogger();

            var secrets = Secrets();
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)
            );

            using var service = new SheetsService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                }
            );

            var request = service.Spreadsheets.Values.Get(spreadsheetId, "'кино'");
            var valueRange = await request.ExecuteAsync();
            var values = valueRange.Values;
            if (values.Count == 0)
            {
                log.Warning("Sheet is empty");
                return;
            }

            var header = new Row(values[0]);
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

            var movies = new List<Movie>();
            for (var i = 1; i < values.Count; i++)
            {
                var row = new Row(values[i]);
                if (row.Has(id) && row.Has(name))
                {
                    var movie = new Movie(
                        row.Get(id),
                        row.Get(name),
                        row.TryGet(originalName),
                        row.TryGet(year).AsNumber()
                    );
                    movies.Add(movie);
                }
                else
                {
                    log.Warning("Row {Index} has no id or name", i);
                }
            }

            var differences = Differences(movies, connection).ToList();
            if (differences.Count == 0)
            {
                log.Information("There are no differences");
                return;
            }

            foreach (var difference in differences)
            {
                log.Information("Difference: {Subject}", difference);
            }

            using var transaction = connection.BeginTransaction();
            using var renameTable = new SQLiteCommand(
                $"ALTER TABLE `movies` RENAME TO `movies_until_{DateTime.UtcNow:O}`",
                connection,
                transaction
            );
            renameTable.ExecuteNonQuery();

            using var createTable = new SQLiteCommand(@"
                CREATE TABLE `movies` (
                    id TEXT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    original_name TEXT,
                    year INT
                )",
                connection,
                transaction
            );
            createTable.ExecuteNonQuery();

            foreach (var movie in movies)
            {
                using var insert = new SQLiteCommand(@"
                    INSERT INTO movies (id, name, original_name, year)
                    VALUES (:id, :name, :original_name, :year)",
                    connection,
                    transaction
                );
                insert.Parameters.AddWithValue("id", movie.Id);
                insert.Parameters.AddWithValue("name", movie.Name);
                insert.Parameters.AddWithValue("original_name", movie.OriginalName);
                insert.Parameters.AddWithValue("year", movie.Year);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private static IEnumerable<string> Differences(List<Movie> movies, SQLiteConnection connection)
        {
            var stored = Stored(connection);
            var join = Full.Join(
                movies,
                stored,
                movie => movie.Id
            );
            foreach (var (key, @new, old) in join)
            {
                if (@new == null)
                {
                    yield return $"{key}: Removed {old.Name}";
                }
                else if (old == null)
                {
                    yield return $"{key}: Added {@new.Name}";
                }
                else
                {
                    if (@new.Name != old.Name)
                    {
                        yield return $"{key}: Name changed from {old.Name} to {@new.Name}";
                    }
                    if (@new.OriginalName != old.OriginalName)
                    {
                        yield return $"{key}: Original name changed from {old.OriginalName} to {@new.OriginalName}";
                    }
                    if (@new.Year != old.Year)
                    {
                        yield return $"{key}: Year changed from {old.Year} to {@new.Year}";
                    }
                }
            }
        }

        private static ClientSecrets Secrets()
        {
            using var file = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
            return GoogleClientSecrets.Load(file).Secrets;
        }

        private static IReadOnlyList<Movie> Stored(SQLiteConnection connection)
        {
            return Yield().ToList();

            IEnumerable<Movie> Yield()
            {
                using var command = new SQLiteCommand(
                    @"SELECT id, name, original_name, year FROM `movies` ORDER BY id",
                    connection
                );
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new Movie(
                        (string)reader["id"],
                        (string)reader["name"],
                        reader["original_name"] as string,
                        reader["year"] as int?
                    );
                }
            }
        }
    }
}