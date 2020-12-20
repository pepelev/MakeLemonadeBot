using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FunBot
{
    public abstract class Sheet
    {
        public abstract Task<IReadOnlyList<Row>> RowsAsync(CancellationToken token);
    }
}