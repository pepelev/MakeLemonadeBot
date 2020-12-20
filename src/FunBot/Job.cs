using System.Threading.Tasks;

namespace FunBot
{
    public abstract class Job
    {
        public abstract Task RunAsync();
    }
}