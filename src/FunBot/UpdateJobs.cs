using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using MakeLemonadeBot.Jobs;
using MakeLemonadeBot.Sheets;
using MakeLemonadeBot.Updates;
using Serilog;

namespace MakeLemonadeBot
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
            yield return Wrap(nameof(SerialsUpdate), Serials());
            yield return Wrap(nameof(BooksUpdate), Books());
            yield return Wrap(nameof(CartoonsUpdate), Cartoons());
            yield return Wrap(nameof(StoreroomUpdate), Storeroom());
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
                                token,
                                connection
                            )
                        )
                    )
                )
            );
        }

        private Job Serials()
        {
            return new SheetDownloading(
                log,
                sheets.Serials,
                token,
                new CancelWatching<IReadOnlyList<Row>>(
                    log,
                    token,
                    new SerialsParsing(
                        log,
                        new DuplicateCheck<Serial>(
                            log,
                            serial => serial.Id,
                            new SerialsUpdate(
                                log,
                                token,
                                connection
                            )
                        )
                    )
                )
            );
        }

        private Job Books()
        {
            return new SheetDownloading(
                log,
                sheets.Books,
                token,
                new CancelWatching<IReadOnlyList<Row>>(
                    log,
                    token,
                    new BooksParsing(
                        log,
                        new DuplicateCheck<Book>(
                            log,
                            book => book.Id,
                            new BooksUpdate(
                                log,
                                token,
                                connection
                            )
                        )
                    )
                )
            );
        }

        private Job Cartoons()
        {
            return new SheetDownloading(
                log,
                sheets.Cartoons,
                token,
                new CancelWatching<IReadOnlyList<Row>>(
                    log,
                    token,
                    new CartoonsParsing(
                        log,
                        new DuplicateCheck<Cartoon>(
                            log,
                            cartoon => cartoon.Id,
                            new CartoonsUpdate(
                                log,
                                token,
                                connection
                            )
                        )
                    )
                )
            );
        }

        private Job Storeroom()
        {
            return new SheetDownloading(
                log,
                sheets.Storeroom,
                token,
                new CancelWatching<IReadOnlyList<Row>>(
                    log,
                    token,
                    new ThingsParsing(
                        log,
                        new DuplicateCheck<Thing>(
                            log,
                            movie => movie.Id,
                            new StoreroomUpdate(
                                log,
                                token,
                                connection
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