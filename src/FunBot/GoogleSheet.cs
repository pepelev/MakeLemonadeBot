using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Configuration;
using FunBot.Conversation;
using Google.Apis.Sheets.v4;

namespace FunBot
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
    }
}