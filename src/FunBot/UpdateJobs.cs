using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using FunBot.Jobs;
using FunBot.Sheets;
using FunBot.Updates;
using Serilog;

namespace FunBot
{
    public sealed class UpdateJobs : IEnumerable<Job>
    {
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly SemaphoreSlim @lock;
        private readonly ILogger log;
        private readonly Sheet.Collection sheets;
        private readonly CancellationToken token;

        public UpdateJobs(
            CancellationToken token,
            SemaphoreSlim @lock,
            ILogger log,
            Clock clock,
            SQLiteConnection connection,
            Sheet.Collection sheets)
        {
            this.token = token;
            this.@lock = @lock;
            this.log = log;
            this.clock = clock;
            this.connection = connection;
            this.sheets = sheets;
        }

        public IEnumerator<Job> GetEnumerator()
        {
            yield return Wrap(nameof(MoviesUpdate), Movies());

            var update1 = new SerialsUpdate(
                connection,
                log,
                sheets.Serials,
                token
            );
            yield return Wrap(nameof(SerialsUpdate), update1);

            var update2 = new BooksUpdate(
                connection,
                log,
                sheets.Books,
                token
            );
            yield return Wrap(nameof(BooksUpdate), update2);
        }

        private Job Movies()
        {
            return new SheetDownloading(
                log,
                sheets.Movies,
                token,
                new CancelWatching<IReadOnlyList<Row>>(
                    log,
                    token,
                    new MoviesParsing(
                        log,
                        new DuplicateCheck<Movie>(
                            log,
                            movie => movie.Id,
                            new MoviesUpdate(
                                log,
                                connection,
                                token
                            )
                        )
                    )
                )
            );
        }

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