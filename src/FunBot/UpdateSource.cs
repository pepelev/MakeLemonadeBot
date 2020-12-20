using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FunBot
{
    public abstract class UpdateSource
    {
        public abstract Task<IReadOnlyList<Update>> UpdatesAsync(CancellationToken token);
    }
}