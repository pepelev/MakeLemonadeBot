using System.Threading.Tasks;

namespace FunBot.Jobs
{
    public abstract class Job
    {
        public abstract Task RunAsync();
    }
}