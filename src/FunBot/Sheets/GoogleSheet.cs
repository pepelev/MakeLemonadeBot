using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Collections;
using FunBot.Configuration;
using Google.Apis.Sheets.v4;

namespace FunBot.Sheets
{
    public sealed class GoogleSheet : Sheet
    {
        private readonly SheetsService service;
        private readonly Source source;

        public GoogleSheet(SheetsService service, Source source)
        {
            this.service = service;
            this.source = source;
        }

        public override async Task<IReadOnlyList<Row>> RowsAsync(CancellationToken token)
        {
            var request = service.Spreadsheets.Values.Get(source.SpreadsheetId, $"'{source.Sheet}'");
            var valueRange = await request.ExecuteAsync(token);
            var values = valueRange.Values;
            return new ProjectingReadOnlyList<IList<object>, Row>(
                values,
                cells => new Row(cells)
            );
        }

        public new sealed class Collection : Sheet.Collection
        {
            private const string MoviesSection = "movies";
            private const string SerialsSection = "serials";
            private const string BooksSection = "books";
            private readonly SheetsService service;
            private readonly Source.Collection sources;

            public Collection(SheetsService service, Source.Collection sources)
            {
                this.service = service;
                this.sources = sources;
            }

            public override Sheet Movies => Sheet(MoviesSection);
            public override Sheet Serials => Sheet(SerialsSection);
            public override Sheet Books => Sheet(BooksSection);

            private Sheet Sheet(string section)
            {
                if (sources.Contains(section))
                    return new GoogleSheet(service, sources.Get(section));

                return new ConstSheet();
            }
        }
    }
}