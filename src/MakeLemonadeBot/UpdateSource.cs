using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MakeLemonadeBot
{
    public abstract class UpdateSource
    {
        public abstract Task<IReadOnlyList<Update>> UpdatesAsync(CancellationToken token);
    }
}