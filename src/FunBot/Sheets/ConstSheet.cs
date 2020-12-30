using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MakeLemonadeBot.Sheets
{
    public sealed class ConstSheet : Sheet
    {
        private readonly Row[] rows;

        public ConstSheet(params Row[] rows)
        {
            this.rows = rows;
        }

        public override Task<IReadOnlyList<Row>> RowsAsync(CancellationToken token) =>
            Task.FromResult<IReadOnlyList<Row>>(rows);
    }
}