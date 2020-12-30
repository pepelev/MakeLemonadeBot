using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MakeLemonadeBot.Sheets
{
    public abstract class Sheet
    {
        public abstract Task<IReadOnlyList<Row>> RowsAsync(CancellationToken token);

        public abstract class Collection
        {
            public abstract Sheet Movies { get; }
            public abstract Sheet Serials { get; }
            public abstract Sheet Books { get; }
            public abstract Sheet Cartoons { get; }
            public abstract Sheet Storeroom { get; }
        }
    }
}