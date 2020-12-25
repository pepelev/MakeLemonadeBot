using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using FunBot.Configuration;
using FunBot.Conversation;
using FunBot.Jobs;
using FunBot.Sheets;
using FunBot.Updates;
using Google.Apis.Sheets.v4;
using Serilog;

namespace FunBot
{
    public sealed class UpdateJobs : IEnumerable<Job>
    {
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly SemaphoreSlim @lock;
        private readonly ILogger log;
        private readonly SheetsService sheets;
        private readonly Source.Collection sources;
        private readonly CancellationToken token;

        public UpdateJobs(
            CancellationToken token,
            SemaphoreSlim @lock,
            ILogger log,
            Clock clock,
            SQLiteConnection connection,
            SheetsService sheets,
            Source.Collection sources)
        {
            this.token = token;
            this.@lock = @lock;
            this.log = log;
            this.clock = clock;
            this.connection = connection;
            this.sheets = sheets;
            this.sources = sources;
        }

        public IEnumerator<Job> GetEnumerator()
        {
            if (sources.Contains("movies"))
            {
                var update = new MoviesUpdate(
                    connection,
                    log,
                    Sheet("movies"),
                    token
                );
                yield return Wrap(nameof(MoviesUpdate), update);
            }
            if (sources.Contains("serials"))
            {
                var update = new SerialsUpdate(
                    connection,
                    log,
                    Sheet("serials"),
                    token
                );
                yield return Wrap(nameof(SerialsUpdate), update);
            }
            if (sources.Contains("books"))
            {
                var update = new BooksUpdate(
                    connection,
                    log,
                    Sheet("books"),
                    token
                );
                yield return Wrap(nameof(BooksUpdate), update);
            }
        }

        private Sheet Sheet(string name) => new GoogleSheet(
            sheets,
            sources.Get(name)
        );

        private Job Wrap(string name, Job job) => new Locking(
            token,
            @lock,
            new Logging(
                name,
                log,
                clock,
                job
            )
        );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}