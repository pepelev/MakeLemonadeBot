using System.Threading.Tasks;

namespace MakeLemonadeBot.Jobs
{
    public abstract class Job
    {
        public abstract Task RunAsync();
    }

    public abstract class Job<T>
    {
        public abstract Task RunAsync(T argument);
    }
}